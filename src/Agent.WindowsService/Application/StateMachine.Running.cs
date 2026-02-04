using System.Text;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Common.Messages;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleRunningEntryAsync()
  {
    _logger.LogInformation("Entering Running state");

    List<Metric> metricCollection = [];
    List<InstructionResult> instrResultsCollection = [];
    IReadOnlyList<Metric> currentCollected = [];
    try
    {
      var authToken = await _secretStore.GetAsync(SecretConfig.AuthTokenKey, Encoding.UTF8, Token);
      var storedInstrResultsBuffer = await _instrStore.GetAllResultsAsync(Token);
      var storedMetricsBuffer = await _metricStore.GetAllAsync(Token);
      var config = await _configStore.GetAsync(Token);

      currentCollected = await _metricCollector.CollectAsync(Token);

      metricCollection.AddRange(storedMetricsBuffer);
      metricCollection.AddRange(currentCollected);
      instrResultsCollection.AddRange(storedInstrResultsBuffer);

      var response = await _serverClient.Post<ReportMessageResponse, ReportMessageRequest>(
        url: UrlConfig.PostReportUrl(config.ServerUrl),
        data: new ReportMessageRequest(
          Metrics: metricCollection.Select(m => m.ToMessage()),
          InstructionResults: instrResultsCollection.Select(ir => ir.ToMessage())
        ),
        metadata: new RequestMetadata(
          AuthToken: authToken,
          AgentName: config.AgentName),
        cancellationToken: Token);

      if(response is null)
        throw new InvalidOperationException("Running state failed: Server response is null");

      await _metricStore.RemoveAllAsync(Token);
      await _instrStore.RemoveAllResultsAsync(Token);

      var newInstructions = response.Instructions.Select(x => x.ToDomain());
      await _instrStore.SaveAsync(newInstructions, Token);

      _logger.LogInformation("Running state completed successfully");
      await _machine.FireAsync(Triggers.RunSuccess);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Running state cancelled");
    }
    catch (HttpRequestException httpEx) when(
      httpEx.StatusCode is System.Net.HttpStatusCode.Unauthorized
                        or System.Net.HttpStatusCode.Forbidden)
    {
      await _metricStore.StoreAsync(currentCollected, Token);

      _logger.LogError(httpEx, "Authentication failure in Running state");
      await _machine.FireAsync(Triggers.AuthFailure);
    }
    catch (Exception ex)
    {
      await _metricStore.StoreAsync(currentCollected, Token);

      _logger.LogError(ex, "Running state failed");
      await _machine.FireAsync(Triggers.RunFailure);
    }
  }

  private async Task HandleRunningExitAsync()
  {
    _logger.LogInformation("Exiting Running state");

    try
    {
      await Task.Delay(5000, Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Running state exit delay cancelled");
    }
  }
}

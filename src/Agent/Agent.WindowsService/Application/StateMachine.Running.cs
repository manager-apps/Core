using System.Text;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Common;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;
public partial class StateMachine
{
  private async Task HandleRunningEntryAsync()
  {
    _logger.LogInformation("Entering RunningStart state");

    List<Metric> metricCollection = [];
    List<InstructionResult> instrResultsCollection = [];
    IReadOnlyList<Metric> currentCollected = [];

    try
    {
      var authToken = await _secretStore.GetAsync(SecretConfig.AuthTokenKey, Encoding.UTF8, CancellationToken.None);
      var storedInstrResultsBuffer = await _instrStore.GetAllResultsAsync(CancellationToken.None);
      var storedMetricsBuffer = await _metricStore.GetAllAsync(CancellationToken.None);
      var config = await _configStore.GetAsync(CancellationToken.None);

      currentCollected = await _metricCollector.CollectAsync(CancellationToken.None);

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
          AgentId: config.AgentId),
        cancellationToken: CancellationToken.None);

      if(response is null)
        throw new InvalidOperationException("Server response is null");

      await _metricStore.RemoveAllAsync(CancellationToken.None);
      await _instrStore.RemoveAllResultsAsync(CancellationToken.None);

      var newInstructions = response.Instructions.Select(x => x.ToDomain());
      await _instrStore.SaveAsync(newInstructions, CancellationToken.None);

      _logger.LogInformation("RunningStart iteration done");
      await _machine.FireAsync(Triggers.RunSuccess);
    }
    catch (HttpRequestException httpEx) when(httpEx.StatusCode is System.Net.HttpStatusCode.Unauthorized or
                                                                  System.Net.HttpStatusCode.Forbidden)
    {
      await _metricStore.StoreAsync(currentCollected, CancellationToken.None);

      _logger.LogError(httpEx, "HTTP Auth error in RunningStart: {StatusCode} - {Message}", httpEx.StatusCode, httpEx.Message);

      await _machine.FireAsync(Triggers.AuthFailure);
    }
    catch (Exception ex)
    {
      await _metricStore.StoreAsync(currentCollected, CancellationToken.None);

      _logger.LogError(ex, "Error in RunningStart");
      await _machine.FireAsync(Triggers.RunFailure);
    }
  }

  private async Task HandleRunningExitAsync()
  {
    _logger.LogInformation("Exiting RunningStart state");

    // Delaying, will be configurable later
    await Task.Delay(5000);
  }
}

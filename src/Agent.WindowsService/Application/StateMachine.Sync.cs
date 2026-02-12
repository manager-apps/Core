using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Common.Messages;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleSynchronizationEntryAsync()
  {
    _logger.LogInformation("Entering Synchronization state");
    try
    {
      var config = await _configStore.GetAsync(Token);

      var hardware = new HardwareMessage(
        OsVersion: Environment.OSVersion.ToString(),
        MachineName: Environment.MachineName,
        ProcessorCount: Environment.ProcessorCount,
        TotalMemoryBytes: GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);

      var configMessage = new ConfigMessage(
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        RunningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        InstructionsExecutionLimit: config.InstructionsExecutionLimit,
        InstructionResultsSendLimit: config.InstructionResultsSendLimit,
        MetricsSendLimit: config.MetricsSendLimit,
        IterationDelaySeconds: config.IterationDelaySeconds,
        AllowedCollectors: config.AllowedCollectors,
        AllowedInstructions: config.AllowedInstructions);

      var syncRequest = new SyncMessageRequest(hardware, configMessage);

      _logger.LogInformation("Synchronizing state with server");

      var response = await _serverClient.Post<SyncMessageResponse, SyncMessageRequest>(
        url: UrlConfig.PostSyncUrl(config.ServerCertificatedUrl),
        data: syncRequest,
        metadata: new RequestMetadata
        {
          AgentName = config.AgentName,
          Headers = new Dictionary<string, string>
          {
            { Common.Headers.AgentVersion, config.Version },
            { Common.Headers.Tag, config.Tag },
          }
        },
        cancellationToken: Token);

      if (response is not null)
      {
        // For now, we won't update the config based on server response since we want to keep the agent's config as the source of truth.
        // _logger.LogInformation("Synchronization completed successfully");
        // var updatedConfig = config with
        // {
        //   AuthenticationExitIntervalSeconds = response.Config.AuthenticationExitIntervalSeconds,
        //   RunningExitIntervalSeconds = response.Config.RunningExitIntervalSeconds,
        //   ExecutionExitIntervalSeconds = response.Config.ExecutionExitIntervalSeconds,
        //   InstructionsExecutionLimit = response.Config.InstructionsExecutionLimit,
        //   InstructionResultsSendLimit = response.Config.InstructionResultsSendLimit,
        //   MetricsSendLimit = response.Config.MetricsSendLimit,
        //   IterationDelaySeconds = response.Config.IterationDelaySeconds,
        //   AllowedCollectors = response.Config.AllowedCollectors.ToList(),
        //   AllowedInstructions = response.Config.AllowedInstructions.ToList()
        // };
        //
        // await _configStore.SaveAsync(updatedConfig, Token);


        await _machine.FireAsync(Triggers.SyncSuccess);
      }
      else
      {
        _logger.LogWarning("Synchronization response is null");
        await _machine.FireAsync(Triggers.SyncFailure);
      }
    }
    catch (HttpRequestException httpEx)
    {
      _logger.LogError(httpEx, "Synchronization HTTP error");
      await _machine.FireAsync(Triggers.SyncFailure);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Synchronization cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Synchronization failed");
      await _machine.FireAsync(Triggers.SyncFailure);
    }
  }

  private Task HandleSynchronizationExitAsync()
  {
    _logger.LogInformation("Exiting Synchronization state");
    return Task.CompletedTask;
  }
}

using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Common.Messages;
using System.Text;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleAuthenticationEntryAsync()
  {
    _logger.LogInformation("Entering Authentication state");
    try
    {
      var clientSecret = await _secretStore.GetAsync(
        SecretConfig.ClientSecretKey,
        Encoding.UTF8, Token);
      var config = await _configStore.GetAsync(Token);

      var hardwareInfo = new HardwareMessage(
        OsVersion: Environment.OSVersion.ToString(),
        MachineName: Environment.MachineName,
        ProcessorCount: Environment.ProcessorCount,
        TotalMemoryBytes: GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);

      var currentConfig = new ConfigMessage(
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        RunningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        InstructionsExecutionLimit: config.InstructionsExecutionLimit,
        InstructionResultsSendLimit: config.InstructionResultsSendLimit,
        MetricsSendLimit: config.MetricsSendLimit,
        IterationDelaySeconds: config.IterationDelaySeconds,
        AllowedCollectors: config.AllowedCollectors,
        AllowedInstructions: config.AllowedInstructions);

      var messageRequest = new AuthMessageRequest(
        AgentName: config.AgentName,
        SecretKey: clientSecret,
        Hardware: hardwareInfo,
        Config: currentConfig);

      var authResponse = await _serverClient.Post<AuthMessageResponse, AuthMessageRequest>(
        url: UrlConfig.PostAuthUrl(config.ServerUrl),
        data: messageRequest,
        metadata: new RequestMetadata
        {
          Headers = new Dictionary<string, string>
          {
            { Common.Headers.AgentVersion, config.Version },
            { Common.Headers.Tag, config.Tag }
          }
        },
        cancellationToken: Token);

      if (authResponse == null
          || string.IsNullOrWhiteSpace(authResponse.AuthToken)
          || string.IsNullOrWhiteSpace(authResponse.RefreshToken))
        throw new InvalidOperationException("Authentication failed: Invalid response from server");

      await _secretStore.SetAsync(SecretConfig.AuthTokenKey, Encoding.UTF8.GetBytes(authResponse.AuthToken), Token);
      await _secretStore.SetAsync(SecretConfig.RefreshTokenKey, Encoding.UTF8.GetBytes(authResponse.RefreshToken), Token);

      if (authResponse.Config is not null)
      {
        var updatedConfig = config with
        {
          AuthenticationExitIntervalSeconds = authResponse.Config.AuthenticationExitIntervalSeconds,
          RunningExitIntervalSeconds = authResponse.Config.RunningExitIntervalSeconds,
          ExecutionExitIntervalSeconds = authResponse.Config.ExecutionExitIntervalSeconds,
          InstructionsExecutionLimit = authResponse.Config.InstructionsExecutionLimit,
          InstructionResultsSendLimit = authResponse.Config.InstructionResultsSendLimit,
          MetricsSendLimit = authResponse.Config.MetricsSendLimit,
          IterationDelaySeconds = authResponse.Config.IterationDelaySeconds,
          AllowedCollectors = authResponse.Config.AllowedCollectors,
          AllowedInstructions = authResponse.Config.AllowedInstructions
        };

        await _configStore.SaveAsync(updatedConfig, Token);
        _logger.LogInformation("Configuration synced from server");
      }

      _logger.LogInformation("Authentication state completed successfully");
      await _machine.FireAsync(Triggers.AuthSuccess);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Authentication state cancelled");
    }
    catch (HttpRequestException httpEx) when (
      httpEx.StatusCode is System.Net.HttpStatusCode.Forbidden
                        or System.Net.HttpStatusCode.Unauthorized)
    {
      _logger.LogWarning("Authentication failed: {Message}", httpEx.Message);
      await _machine.FireAsync(Triggers.AuthFailure);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Authentication state failed");
      await _machine.FireAsync(Triggers.AuthFailure);
    }
  }

  private async Task HandleAuthenticationExitAsync()
  {
    _logger.LogInformation("Exiting Authentication state");

    try
    {
      var config = await _configStore.GetAsync(Token);
      await Task.Delay(TimeSpan.FromSeconds(config.AuthenticationExitIntervalSeconds), Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Authentication state exit delay cancelled");
    }
  }
}

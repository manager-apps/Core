using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using System.Text;
using Common.Messages;

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

      var authResponse = await _serverClient.Post<AuthMessageResponse, AuthMessageRequest>(
        url: UrlConfig.PostAuthUrl(config.ServerUrl),
        data: new AuthMessageRequest(
          AgentName: config.AgentName,
          SecretKey: clientSecret),
        metadata: new RequestMetadata
        {
          Headers = new Dictionary<string, string>
          {
            { Common.Headers.Version, config.Version },
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

using System.Text;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Common.Messages;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleAuthenticationEntryAsync()
  {
    _logger.LogInformation("Entering Authentication state");
    try
    {
      var config = await _configStore.GetAsync(CancellationToken.None);
      var clientSecret = await _secretStore.GetAsync(SecretConfig.ClientSecretKey, Encoding.UTF8);

      var authResponse = await _serverClient.Post<AuthMessageResponse, AuthMessageRequest>(
        url: UrlConfig.PostAuthUrl(config.ServerUrl),
        data: new AuthMessageRequest(
          AgentName: config.AgentName,
          SecretKey: clientSecret),
        metadata: new RequestMetadata(),
        cancellationToken: CancellationToken.None);

      if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AuthToken) || string.IsNullOrWhiteSpace(authResponse.RefreshToken))
        throw new InvalidOperationException("Authentication failed: Invalid response from server");

      await _secretStore.SetAsync(SecretConfig.AuthTokenKey, Encoding.UTF8.GetBytes(authResponse.AuthToken));
      await _secretStore.SetAsync(SecretConfig.RefreshTokenKey, Encoding.UTF8.GetBytes(authResponse.RefreshToken));

      await _machine.FireAsync(Triggers.AuthSuccess);
    }
    catch (HttpRequestException httpEx) when (httpEx.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized)
    {
      _logger.LogWarning("Authentication failed: {Message}", httpEx.Message);
      await _machine.FireAsync(Triggers.AuthFailure);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in Authentication");
      await _machine.FireAsync(Triggers.AuthFailure);
    }
  }

  private async Task HandleAuthenticationExitAsync()
  {
    _logger.LogInformation("Exiting Authentication state");

    // Delaying, will be configurable later
    await Task.Delay(5000);
  }
}

using System.Net;
using System.Net.Http.Json;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Communication;

public class HttpServerClient (
  ILogger<HttpServerClient> logger,
  HttpClient httpClient
) : IServerClient
{
  public async Task<TResponse?> Post<TResponse, TRequest>(
    string url,
    TRequest data,
    RequestMetadata metadata, CancellationToken cancellationToken)
  {
    using var request = new HttpRequestMessage(HttpMethod.Post, url);
    request.Content = JsonContent.Create(data);

    if (!string.IsNullOrEmpty(metadata.AuthToken))
      request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", metadata.AuthToken);

    if (!string.IsNullOrEmpty(metadata.AgentName))
      request.Headers.Add("X-Agent-Id", metadata.AgentName);

    foreach (var (key, value) in metadata.Headers)
    {
      if (!request.Headers.TryAddWithoutValidation(key, value))
        logger.LogWarning("Failed to add header {HeaderKey} to request", key);
    }

    logger.LogDebug("Sending POST request to {Url} with AgentName: {AgentName}", url, metadata.AgentName);

    HttpResponseMessage response;
    try
    {
      response = await httpClient.SendAsync(request, cancellationToken);
    }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
    {
      logger.LogError("Request to {Url} timed out", url);
      throw new HttpRequestException($"Request to {url} timed out", ex);
    }
    catch (HttpRequestException ex)
    {
      logger.LogError(ex, "Network error while sending request to {Url}", url);
      throw;
    }

    if (!response.IsSuccessStatusCode)
    {
      var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

      logger.LogError(
        "Request to {Url} failed with status {StatusCode}. Response: {Response}",
        url, response.StatusCode, errorContent);

      throw response.StatusCode switch
      {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
          new HttpRequestException("Authentication failed", null, response.StatusCode),
        HttpStatusCode.BadRequest =>
          new HttpRequestException($"Bad request: {errorContent}", null, response.StatusCode),
        HttpStatusCode.NotFound =>
          new HttpRequestException($"Endpoint not found: {url}", null, response.StatusCode),
        HttpStatusCode.ServiceUnavailable =>
          new HttpRequestException("Server is unavailable", null, response.StatusCode),
        _ when ((int)response.StatusCode >= 500) =>
          new HttpRequestException($"Server error: {response.StatusCode}", null, response.StatusCode),
        _ => new HttpRequestException($"Request failed with status {response.StatusCode}", null, response.StatusCode)
      };
    }

    var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);

    logger.LogInformation("POST request to {Url} completed successfully with status {StatusCode}", url, response.StatusCode);
    return result;
  }
}

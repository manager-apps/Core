namespace Agent.WindowsService.Config;

public static class UrlConfig
{
  /// <summary>
  /// API version used in the URLs.
  /// </summary>
  private const string Version = "v1";

  /// <summary>
  /// Constructs the URL for posting authentication requests to the agent API.
  /// </summary>
  public static string PostAuthUrl(string baseUrl)
    => $"{baseUrl.TrimEnd('/')}/agent/api/{Version}/auth";

  /// <summary>
  /// Constructs the URL for posting reports to the agent API.
  /// </summary>
  public static string PostReportUrl(string baseUrl)
    => $"{baseUrl.TrimEnd('/')}/agent/api/{Version}/report";
}

/// <summary>
/// Metadata for requests sent by the agent.
/// </summary>
/// <param name="AgentName"></param>
/// <param name="AuthToken"></param>
public record RequestMetadata(
  string? AgentName = null,
  string? AuthToken = null);

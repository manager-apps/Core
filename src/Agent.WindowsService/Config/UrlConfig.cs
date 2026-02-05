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
    => $"{baseUrl.TrimEnd('/')}/api/{Version}/agent/auth";

  /// <summary>
  /// Constructs the URL for posting reports to the agent API.
  /// </summary>
  public static string PostReportUrl(string baseUrl)
    => $"{baseUrl.TrimEnd('/')}/api/{Version}/agent/report";
}

/// <summary>
/// Metadata for requests sent by the agent.
/// </summary>
/// <param name="AgentName"></param>
/// <param name="AuthToken"></param>
public record RequestMetadata
{
  public string? AgentName { get; init; } = null;
  public string? AuthToken { get; init; } = null;
  public Dictionary<string, string> Headers { get; init; } = new();
}

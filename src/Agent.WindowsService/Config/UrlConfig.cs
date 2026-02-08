namespace Agent.WindowsService.Config;

public static class UrlConfig
{
  /// <summary>
  /// API version used in the URLs.
  /// </summary>
  private const string Version = "v1";

  /// <summary>
  /// Constructs the URL for posting reports to the agent API.
  /// </summary>
  public static string PostReportUrl(string baseUrl)
    => $"{baseUrl}/api/{Version}/agents/report";
}

/// <summary>
/// Metadata for requests sent by the agent.
/// </summary>
public record RequestMetadata
{
  public string? AgentName { get; init; }
  public Dictionary<string, string> Headers { get; init; } = new();
}

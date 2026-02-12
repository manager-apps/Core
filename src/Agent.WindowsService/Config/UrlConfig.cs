namespace Agent.WindowsService.Config;

public static class UrlConfig
{
  private const string Version = "v1";

  /// <summary>
  /// Constructs the URL for posting reports to the agent API.
  /// </summary>
  public static string PostReportUrl(string baseUrl)
    => $"{baseUrl}/api/{Version}/report";

  /// <summary>
  /// Constructs the URL for posting enrollment requests to the agent API.
  /// </summary>
  public static string PostEnrollmentUrl(string baseUrl)
    => $"{baseUrl}/api/{Version}/certificates/enroll/token";

  /// <summary>
  /// Constructs the URL for posting certificate renewal requests to the agent API.
  /// </summary>
  public static string PostRenewalUrl(string baseUrl)
    => $"{baseUrl}/api/{Version}/certificates/renew";

  /// <summary>
  /// Constructs the URL for checking certificate revocation status to the agent API.
  /// </summary>
  public static string GetRevocationCheckUrl(string baseUrl, string thumbprint)
    => $"{baseUrl}/api/{Version}/certificates/revocation/{thumbprint}";

  /// <summary>
  /// Constructs the URL for posting synchronization requests to the agent API.
  /// </summary>
  public static string PostSyncUrl(string baseUrl)
    => $"{baseUrl.TrimEnd('/')}/api/{Version}/sync";
}

/// <summary>
/// Metadata for requests sent by the agent.
/// </summary>
public record RequestMetadata
{
  public string? AgentName { get; init; }
  public Dictionary<string, string> Headers { get; init; } = new();
}

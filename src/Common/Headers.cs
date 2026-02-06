namespace Common;

/// <summary>
/// Custom HTTP header names used for agent-server communication.
/// </summary>
public static class Headers
{
  /// <summary>
  /// The version of the agent software (e.g., "1.0.0", "2.1.0").
  /// Not to be confused with API version which is in the URL path.
  /// </summary>
  public const string AgentVersion = "X-Agent-Version";

  /// <summary>
  /// The deployment tag of the agent (e.g., "production", "staging").
  /// </summary>
  public const string Tag = "X-Tag";
}

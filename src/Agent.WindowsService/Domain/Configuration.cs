namespace Agent.WindowsService.Domain;

public record Configuration
{
  /// <summary>
  /// Unique identifier for the agent
  /// </summary>
  public string AgentName { get; set;  } = string.Empty;

  /// <summary>
  /// URL of the server the agent communicates with
  /// </summary>
  public string ServerUrl { get; set; } = string.Empty;
}

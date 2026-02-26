namespace Agent.WindowsService.Domain;

public record Configuration
{
  public string Version { get; set; } = string.Empty;
  public string AgentName { get; set;  } = string.Empty;
  public string ServerCertificatedUrl { get; set; } = string.Empty;
  public string ServerNotCertificatedUrl { get; set; } = string.Empty;
  public string Tag { get; set; } = "default";
  public int AuthenticationExitIntervalSeconds { get; set; } = 10;
  public int RunningExitIntervalSeconds { get; set; } = 10;
  public int ExecutionExitIntervalSeconds { get; set; } = 10;
  public int InstructionsExecutionLimit { get; set; } = 15;
  public int InstructionResultsSendLimit { get; set; } = 15;
  public int MetricsSendLimit { get; set; } = 20;
  public int IterationDelaySeconds { get; set; } = 20;
  public IReadOnlyList<string> AllowedCollectors { get; set; } = ["cpu_usage", "memory_usage", "disk_usage"];
  public IReadOnlyList<string> AllowedInstructions { get; set; } = ["Shell", "Gpo", "Config"];
  public string? EnrollmentToken { get; set; }
}

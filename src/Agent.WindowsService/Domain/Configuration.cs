namespace Agent.WindowsService.Domain;

public record Configuration
{
  public string Version { get; set; } = string.Empty;
  public string AgentName { get; set;  } = string.Empty;
  public string ServerUrl { get; set; } = string.Empty;
  public string Tag { get; set; } = "default";

  public int AuthenticationExitIntervalSeconds { get; set; } = 5;
  public int SynchronizationExitIntervalSeconds { get; set; } = 5;
  public int RunningExitIntervalSeconds { get; set; } = 5;
  public int ExecutionExitIntervalSeconds { get; set; } = 5;
  public int InstructionsExecutionLimit { get; set; } = 10;
  public int InstructionResultsSendLimit { get; set; } = 10;
  public int MetricsSendLimit { get; set; } = 10;
  public IReadOnlyList<string> AllowedCollectors { get; set; } = ["cpu_usage", "memory_usage", "disk_usage"];
  public IReadOnlyList<string> AllowedInstructions { get; set; } = ["ShellCommand", "GpoSet", "Config"];
}

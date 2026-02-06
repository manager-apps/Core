namespace Common.Messages;

public record AuthMessageResponse(
  string AuthToken,
  string RefreshToken,
  ConfigMessage? Config = null);

public record AuthMessageRequest(
  string AgentName,
  string SecretKey,
  HardwareMessage Hardware,
  ConfigMessage Config);

/// <summary>
/// Hardware information collected from the agent machine
/// </summary>
public record HardwareMessage(
  string OsVersion,
  string MachineName,
  int ProcessorCount,
  long TotalMemoryBytes);

/// <summary>
/// Agent configuration that can be synced between server and agent
/// </summary>
public record ConfigMessage(
  int AuthenticationExitIntervalSeconds,
  int RunningExitIntervalSeconds,
  int ExecutionExitIntervalSeconds,
  int InstructionsExecutionLimit,
  int InstructionResultsSendLimit,
  int MetricsSendLimit,
  int IterationDelaySeconds,
  IReadOnlyList<string> AllowedCollectors,
  IReadOnlyList<string> AllowedInstructions);

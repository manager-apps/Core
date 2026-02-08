namespace Common.Messages;

public record HardwareMessage(
  string OsVersion,
  string MachineName,
  int ProcessorCount,
  long TotalMemoryBytes);

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

/// <summary>
/// Request to synchronize agent state with server.
/// </summary>
public record SyncMessageRequest(
  HardwareMessage Hardware,
  ConfigMessage Config);

/// <summary>
/// Response from server with updated configuration.
/// </summary>
public record SyncMessageResponse(
  ConfigMessage Config);

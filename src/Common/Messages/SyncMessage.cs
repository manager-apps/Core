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

public record SyncMessageRequest(
  HardwareMessage Hardware,
  ConfigMessage Config);

public record SyncMessageResponse(
  ConfigMessage Config);

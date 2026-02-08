namespace Server.Api.Features.Config;

public record ConfigResponse(
  long Id,
  long AgentId,
  int IterationDelaySeconds,
  int AuthenticationExitIntervalSeconds,
  int RunningExitIntervalSeconds,
  int ExecutionExitIntervalSeconds,
  int InstructionsExecutionLimit,
  int InstructionResultsSendLimit,
  int MetricsSendLimit,
  List<string> AllowedCollectors,
  List<string> AllowedInstructions);

public record ConfigUpdateRequest(
  int? AuthenticationExitIntervalSeconds = null,
  int? RunningExitIntervalSeconds = null,
  int? ExecutionExitIntervalSeconds = null,
  int? InstructionsExecutionLimit = null,
  int? InstructionResultsSendLimit = null,
  int? MetricsSendLimit = null,
  List<string>? AllowedCollectors = null,
  List<string>? AllowedInstructions = null);


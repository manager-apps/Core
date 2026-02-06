namespace Server.Api.Features.Config;

/// <summary>
/// Response model for configuration settings of an agent.
/// </summary>
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

/// <summary>
/// Request model for updating agent configuration.
/// All fields are optional - only provided fields will be updated.
/// </summary>
public record ConfigUpdateRequest(
  int? AuthenticationExitIntervalSeconds = null,
  int? RunningExitIntervalSeconds = null,
  int? ExecutionExitIntervalSeconds = null,
  int? InstructionsExecutionLimit = null,
  int? InstructionResultsSendLimit = null,
  int? MetricsSendLimit = null,
  List<string>? AllowedCollectors = null,
  List<string>? AllowedInstructions = null);


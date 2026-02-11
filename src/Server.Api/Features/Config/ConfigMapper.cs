using Common.Messages;

namespace Server.Api.Features.Config;

internal static class ConfigMapper
{
  extension(Domain.Config config)
  {
    internal ConfigMessage ToMessage()
      => new(
        IterationDelaySeconds: config.IterationDelaySeconds,
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        RunningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        InstructionsExecutionLimit: config.InstructionsExecutionLimit,
        InstructionResultsSendLimit: config.InstructionResultsSendLimit,
        MetricsSendLimit: config.MetricsSendLimit,
        AllowedCollectors: config.GetAllowedCollectorsList(),
        AllowedInstructions: config.GetAllowedInstructionsList());

    internal ConfigResponse ToResponse()
      => new(
        Id: config.Id,
        AgentId: config.AgentId,
        IterationDelaySeconds: config.IterationDelaySeconds,
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        RunningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        InstructionsExecutionLimit: config.InstructionsExecutionLimit,
        InstructionResultsSendLimit: config.InstructionResultsSendLimit,
        MetricsSendLimit: config.MetricsSendLimit,
        AllowedCollectors: config.GetAllowedCollectorsList().ToList(),
        AllowedInstructions: config.GetAllowedInstructionsList().ToList());
  }
}

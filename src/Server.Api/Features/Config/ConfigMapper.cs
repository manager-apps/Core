using Common.Messages;

namespace Server.Api.Features.Config;

public static class ConfigMapper
{
  extension(Domain.Config config)
  {
    public ConfigMessage ToMessage()
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

    public ConfigResponse ToResponse()
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

  extension(ConfigMessage config)
  {
    public Domain.Config ToDomain()
      => Domain.Config.Create(
        iterationDelaySeconds: config.IterationDelaySeconds,
        authenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        runningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        executionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        instructionsExecutionLimit: config.InstructionsExecutionLimit,
        instructionResultsSendLimit: config.InstructionResultsSendLimit,
        metricsSendLimit: config.MetricsSendLimit,
        allowedCollectors: config.AllowedCollectors,
        allowedInstructions: config.AllowedInstructions);
  }
}

using Common.Messages;

namespace Server.Api.Features.Config;

public static class ConfigMapper
{
  extension(Domain.Config config)
  {
    public ConfigMessage ToMessage()
      => new(
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        SynchronizationExitIntervalSeconds: config.SynchronizationExitIntervalSeconds,
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
        AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        SynchronizationExitIntervalSeconds: config.SynchronizationExitIntervalSeconds,
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
        authenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
        synchronizationExitIntervalSeconds: config.SynchronizationExitIntervalSeconds,
        runningExitIntervalSeconds: config.RunningExitIntervalSeconds,
        executionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
        instructionsExecutionLimit: config.InstructionsExecutionLimit,
        instructionResultsSendLimit: config.InstructionResultsSendLimit,
        metricsSendLimit: config.MetricsSendLimit,
        allowedCollectors: config.AllowedCollectors,
        allowedInstructions: config.AllowedInstructions);
  }
}

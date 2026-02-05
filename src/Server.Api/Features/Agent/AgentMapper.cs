using Common.Messages;
using Server.Domain;

namespace Server.Api.Features.Agent;

public static class AgentMapper
{
  extension(ConfigMessage config)
  {
    public Config ToDomain()
      => Config.Create(
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

  extension(HardwareMessage hardware)
  {
    public Hardware ToDomain()
      => Hardware.Create(
        osVersion: hardware.OsVersion,
        machineName: hardware.MachineName,
        processorCount: hardware.ProcessorCount,
        totalMemoryBytes: hardware.TotalMemoryBytes);
  }

  extension(AuthMessageRequest request)
  {
    public Server.Domain.Agent ToDomain(
      byte[] secretKeyHash,
      byte[] secretKeySalt,
      string tag,
      string version)
      => Domain.Agent.Create(
        config: request.Config.ToDomain(),
        hardware: request.Hardware.ToDomain(),
        name: request.AgentName,
        sourceTag: tag,
        secretKeyHash: secretKeyHash,
        secretKeySalt: secretKeySalt);
  }

  extension(Server.Domain.Agent agent)
  {
    public AgentResponse ToResponse()
      => new(
        Id: agent.Id,
        Name: agent.Name,
        SourceTag: agent.SourceTag,
        CurrentTag: agent.CurrentTag,
        State: agent.State,
        CreatedAt: agent.CreatedAt,
        LastUpdatedAt: agent.LastSeenAt,
        UpdatedAt: agent.UpdatedAt);
  }

  extension(Config config)
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
  }
}

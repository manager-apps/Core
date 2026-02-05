using System.Text.Json;
using Common;
using Common.Messages;

namespace Server.Api.Features.Instruction;

public static class InstructionMapper
{
  extension(Server.Domain.Instruction instruction)
  {
    public InstructionResponse ToResponse() =>
      new(
        Id: instruction.Id,
        AgentId: instruction.AgentId,
        Type: instruction.Type,
        PayloadJson: instruction.PayloadJson,
        State: instruction.State,
        Output: instruction.Output,
        Error: instruction.Error,
        CreatedAt: instruction.CreatedAt,
        UpdatedAt: instruction.UpdatedAt);
  }

  extension(CreateInstructionRequest request)
  {
    public Server.Domain.Instruction ToDomain() =>
      Server.Domain.Instruction.Create(
        agentId: request.AgentId,
        type: request.Type,
        payloadJson: request.PayloadJson);
  }

  extension(CreateAgentInstructionRequest request)
  {
    public Server.Domain.Instruction ToDomain(long agentId) =>
      Domain.Instruction.Create(
        agentId: agentId,
        type: request.Type,
        payloadJson: request.PayloadJson);
  }

  extension(CreateShellCommandRequest request)
  {
    public Server.Domain.Instruction ToDomain(long agentId)
    {
      var payload = new ShellCommandPayload(request.Command, request.Timeout);
      var payloadJson = JsonSerializer.Serialize<InstructionPayload>(payload, JsonOptions.Default);
      return Domain.Instruction.Create(
        agentId: agentId,
        type: Domain.InstructionType.ShellCommand,
        payloadJson: payloadJson);
    }
  }

  extension(CreateGpoSetRequest request)
  {
    public Server.Domain.Instruction ToDomain(long agentId)
    {
      var payload = new GpoSetPayload(request.Name, request.Value);
      var payloadJson = JsonSerializer.Serialize<InstructionPayload>(payload, JsonOptions.Default);
      return Domain.Instruction.Create(
        agentId: agentId,
        type: Domain.InstructionType.GpoSet,
        payloadJson: payloadJson);
    }
  }

  extension(CreateConfigSyncRequest request)
  {
    public Server.Domain.Instruction ToDomain(long agentId, ConfigMessage currentConfig)
    {
      var configMessage = new ConfigMessage(
        AuthenticationExitIntervalSeconds: request.AuthenticationExitIntervalSeconds ?? currentConfig.AuthenticationExitIntervalSeconds,
        SynchronizationExitIntervalSeconds: request.SynchronizationExitIntervalSeconds ?? currentConfig.SynchronizationExitIntervalSeconds,
        RunningExitIntervalSeconds: request.RunningExitIntervalSeconds ?? currentConfig.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds: request.ExecutionExitIntervalSeconds ?? currentConfig.ExecutionExitIntervalSeconds,
        InstructionsExecutionLimit: request.InstructionsExecutionLimit ?? currentConfig.InstructionsExecutionLimit,
        InstructionResultsSendLimit: request.InstructionResultsSendLimit ?? currentConfig.InstructionResultsSendLimit,
        MetricsSendLimit: request.MetricsSendLimit ?? currentConfig.MetricsSendLimit,
        AllowedCollectors: request.AllowedCollectors ?? currentConfig.AllowedCollectors,
        AllowedInstructions: request.AllowedInstructions ?? currentConfig.AllowedInstructions);

      var payload = new ConfigPayload(configMessage);
      var payloadJson = JsonSerializer.Serialize<InstructionPayload>(payload, JsonOptions.Default);
      return Domain.Instruction.Create(
        agentId: agentId,
        type: Domain.InstructionType.Config,
        payloadJson: payloadJson);
    }
  }
}

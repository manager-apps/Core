using System.Text.Json;
using Common.Messages;
using Common;

namespace Server.Api.Features.Instruction;

internal static class InstructionMapper
{
  extension(Server.Domain.Instruction instruction)
  {
    internal InstructionResponse ToResponse() =>
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

  extension(CreateShellCommandRequest request)
  {
    internal Server.Domain.Instruction ToDomain(long agentId)
    {
      var payload = new ShellCommandPayload(request.Command, request.Timeout);
      var payloadJson = JsonSerializer
        .Serialize<InstructionPayload>(payload, JsonOptions.Default);
      return Domain.Instruction.Create(
        agentId: agentId,
        type: Domain.InstructionType.Shell,
        payloadJson: payloadJson);
    }
  }

  extension(CreateGpoSetRequest request)
  {
    internal Server.Domain.Instruction ToDomain(long agentId)
    {
      var payload = new GpoSetPayload(request.Name, request.Value);
      var payloadJson = JsonSerializer
        .Serialize<InstructionPayload>(payload, JsonOptions.Default);
      return Domain.Instruction.Create(
        agentId: agentId,
        type: Domain.InstructionType.Gpo,
        payloadJson: payloadJson);
    }
  }
}

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
      Server.Domain.Instruction.Create(
        agentId: agentId,
        type: request.Type,
        payloadJson: request.PayloadJson);
  }
}

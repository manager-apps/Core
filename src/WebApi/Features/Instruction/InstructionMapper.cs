namespace WebApi.Features.Instruction;

public static class InstructionMapper
{
  extension(Domain.Instruction instruction)
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
    public Domain.Instruction ToDomain() =>
      Domain.Instruction.Create(
        agentId: request.AgentId,
        type: request.Type,
        payloadJson: request.PayloadJson);
  }

  extension(CreateAgentInstructionRequest request)
  {
    public Domain.Instruction ToDomain(long agentId) =>
      Domain.Instruction.Create(
        agentId: agentId,
        type: request.Type,
        payloadJson: request.PayloadJson);
  }
}

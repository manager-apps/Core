using Common.Messages;

namespace Agent.WindowsService.Abstraction;

public static class ToDomainMapper
{
  public static Domain.Instruction ToDomain(this InstructionMessage instructionMessage)
    => new()
    {
      AssociativeId = instructionMessage.AssociatedId,
      Type = (Domain.InstructionType)instructionMessage.Type,
      Payload = instructionMessage.Payload
    };
}

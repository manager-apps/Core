using Agent.WindowsService.Domain;
using Common.Messages;

namespace Agent.WindowsService.Abstraction;

public static class ToDomainMapper
{
  extension(InstructionMessage instructionMessage)
  {
    public Instruction ToDomain()
      => new()
      {
        AssociativeId = instructionMessage.AssociatedId,
        Type = (InstructionType)instructionMessage.Type,
        Payload = instructionMessage.Payload
      };
  }
}

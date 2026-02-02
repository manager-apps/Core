using Common.Messages;

namespace Agent.WindowsService.Abstraction;

public static class FromDomainMapper
{
  extension(Domain.Metric message)
  {
    public MetricMessage ToMessage()
      => new(
        Type: message.Type,
        Name: message.Name,
        Value: message.Value,
        Unit: message.Unit,
        TimestampUtc: message.TimestampUtc,
        Metadata: message.Metadata);
  }

  extension(Domain.InstructionResult instruction)
  {
    public InstructionResultMessage ToMessage()
      => new(
        AssociatedId: instruction.AssociativeId,
        Success: instruction.Success,
        Output: instruction.Output,
        Error: instruction.Error);
  }
}


using Common.Messages;

namespace Agent.WindowsService.Abstraction;

public static class FromDomainMapper
{
  public static MetricMessage ToMessage(this Domain.Metric metric)
    => new(
      Type: metric.Type,
      Name: metric.Name,
      Value: metric.Value,
      Unit: metric.Unit,
      TimestampUtc: metric.TimestampUtc,
      Metadata: metric.Metadata);

  public static InstructionResultMessage ToMessage(this Domain.InstructionResult instruction)
    => new(
      AssociatedId: instruction.AssociativeId,
      Success: instruction.Success,
      Output: instruction.Output,
      Error: instruction.Error);
}


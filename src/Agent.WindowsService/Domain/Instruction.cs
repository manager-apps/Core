using Common.Messages;

namespace Agent.WindowsService.Domain;

public enum InstructionType
{
  GpoSet = 1,
  ShellCommand = 2,
}

public class Instruction
{
  public long Id { get; set; }

  /// <summary>
  /// Unique identifier for the instruction
  /// </summary>
  public long AssociativeId { get; set; }

  /// <summary>
  /// Type of instruction
  /// </summary>
  public required InstructionType Type { get; set; }

  /// <summary>
  /// Typed payload containing instruction details
  /// </summary>
  public required InstructionPayload Payload { get; set; }
}

public class InstructionResult
{
  /// <summary>
  /// Identifier of the instruction
  /// </summary>
  public required long AssociativeId { get; set; }

  /// <summary>
  /// Success status of the instruction execution
  /// </summary>
  public required bool Success { get; set; }

  /// <summary>
  /// Message or output from the instruction execution
  /// </summary>
  public string? Output { get; set; }

  /// <summary>
  /// Message or output from the instruction execution
  /// </summary>
  public string? Error { get; set; }
}



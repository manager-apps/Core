using System.Text.Json;
using Common.Messages;

namespace Agent.WindowsService.Domain;

public enum InstructionType
{
  /// <summary>
  /// Group Policy Object setting instruction
  /// </summary>
  GpoSet = 1,

  /// <summary>
  /// Shell command execution instruction
  /// </summary>
  ShellCommand = 2,
}

public class Instruction
{
  /// <summary>
  /// Unique identifier for the instruction
  /// </summary>
  public long AssociativeId { get; init; }

  /// <summary>
  /// Type of instruction
  /// </summary>
  public required InstructionType Type { get; init; }

  /// <summary>
  /// Typed payload containing instruction details
  /// </summary>
  public required InstructionPayload Payload { get; init; }
}

public class InstructionResult
{
  /// <summary>
  /// Identifier of the instruction
  /// </summary>
  public required long AssociativeId { get; init; }

  /// <summary>
  /// Success status of the instruction execution
  /// </summary>
  public required bool Success { get; init; }

  /// <summary>
  /// Message or output from the instruction execution
  /// </summary>
  public string? Output { get; init; }

  /// <summary>
  /// Message or output from the instruction execution
  /// </summary>
  public string? Error { get; init; }
}



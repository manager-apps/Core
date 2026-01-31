using System.Text.Json;
using Common.Messages;

namespace Agent.WindowsService.Domain;

public enum InstructionType
{
  GpoSet = 1,
  ShellCommand = 2,
}

public class Instruction
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = false
  };

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

  /// <summary>
  /// Deserializes the instruction payload from a JSON string based on the instruction type.
  /// </summary>
  public static InstructionPayload DeserializePayload(InstructionType type, string json)
  {
    return type switch
    {
      InstructionType.ShellCommand => JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
                                      ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),
      InstructionType.GpoSet => JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
                                ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),
      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }

  /// <summary>
  /// Serializes the instruction payload to a JSON string.
  /// </summary>
  public static string SerializePayload(InstructionPayload payload)
    => JsonSerializer.Serialize(payload, JsonOptions);
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



using System.Text.Json;
using Common.Messages;

namespace Server.Api.Features.Instruction;

public static class InstructionUtils
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    WriteIndented = false
  };

  /// <summary>
  /// Deserializes the instruction payload from the given JSON string based on the instruction type.
  /// </summary>
  public static InstructionPayload DeserializePayload(Server.Domain.InstructionType type, string json)
  {
    return type switch
    {
      Server.Domain.InstructionType.ShellCommand =>
        JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),

      Server.Domain.InstructionType.GpoSet =>
        JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),
      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }

  /// <summary>
  /// Serializes the given instruction payload to a JSON string.
  /// </summary>
  public static string SerializePayload(InstructionPayload payload)
    => JsonSerializer.Serialize(payload, JsonOptions);
}

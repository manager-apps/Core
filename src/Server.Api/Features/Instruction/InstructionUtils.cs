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
      Domain.InstructionType.ShellCommand =>
        JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),

      Domain.InstructionType.GpoSet =>
        JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),

      Domain.InstructionType.Config =>
        JsonSerializer.Deserialize<ConfigPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ConfigPayload)}"),

      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }
}

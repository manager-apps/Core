using System.Text.Json;
using Agent.WindowsService.Domain;
using Common.Messages;

namespace Agent.WindowsService.Utils;

public static class InstructionUtils
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = false
  };

  /// <summary>
  /// Deserializes the instruction payload based on the instruction type.
  /// </summary>
  public static InstructionPayload DeserializePayload(InstructionType type, string json)
  {
    return type switch
    {
      InstructionType.Shell
        => JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
              ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),
      InstructionType.Gpo =>
        JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
          ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),

      InstructionType.Config =>
        JsonSerializer.Deserialize<ConfigPayload>(json, JsonOptions)
          ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ConfigPayload)}"),
      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }

  /// <summary>
  /// Serializes the instruction payload to a JSON string.
  /// </summary>
  public static string SerializePayload(InstructionPayload payload)
    => JsonSerializer.Serialize(payload, JsonOptions);
}

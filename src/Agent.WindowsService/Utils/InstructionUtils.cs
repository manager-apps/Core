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

  public static string SerializePayload(InstructionPayload payload)
    => JsonSerializer.Serialize(payload, JsonOptions);
}

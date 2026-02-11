using System.Text.Json.Serialization;

namespace Common.Messages;

public record InstructionResultMessage(
  long AssociatedId,
  bool Success,
  string? Output,
  string? Error);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ShellCommandPayload), "shell")]
[JsonDerivedType(typeof(GpoSetPayload), "gpo")]
[JsonDerivedType(typeof(ConfigPayload), "config")]
public abstract record InstructionPayload;

public record ShellCommandPayload(
  string Command,
  int Timeout = 5000) : InstructionPayload;

public record GpoSetPayload(
  string Name,
  string Value
) : InstructionPayload;

public record ConfigPayload(
  ConfigMessage Config
) : InstructionPayload;

public record InstructionMessage(
  long AssociatedId,
  int Type,
  InstructionPayload Payload);

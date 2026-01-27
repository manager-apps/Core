using System.Text.Json.Serialization;

namespace Common.Messages;

public record InstructionResultMessage(
  long AssociatedId,
  bool Success,
  string? Output,
  string? Error);

/// <summary>
/// Base class for instruction payloads using discriminated union pattern.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ShellCommandPayload), "shell")]
[JsonDerivedType(typeof(GpoSetPayload), "gpo")]
public abstract record InstructionPayload;

/// <summary>
/// Payload for shell command execution.
/// </summary>
public record ShellCommandPayload(
  string Command,
  int Timeout = 5000) : InstructionPayload;

/// <summary>
/// Payload for GPO (Group Policy Object) settings.
/// </summary>
public record GpoSetPayload(
  string Name,
  string Value) : InstructionPayload;

/// <summary>
/// Message representing an instruction sent to an agent.
/// </summary>
public record InstructionMessage(
  long AssociatedId,
  int Type,
  InstructionPayload Payload);

using System.Text.Json.Serialization;

namespace Common.Messages;

/// <summary>
/// Message representing the result of an instruction execution, sent from agent to server.
/// </summary>
/// <param name="AssociatedId"></param>
/// <param name="Success"></param>
/// <param name="Output"></param>
/// <param name="Error"></param>
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
[JsonDerivedType(typeof(ConfigPayload), "config")]
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
  string Value
) : InstructionPayload;

/// <summary>
/// Payload for configuration update.
/// </summary>
/// <param name="Config"></param>
public record ConfigPayload(
  ConfigMessage Config
) : InstructionPayload;

/// <summary>
/// Message representing an instruction sent to an agent.
/// </summary>
public record InstructionMessage(
  long AssociatedId,
  int Type,
  InstructionPayload Payload);

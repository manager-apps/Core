using Server.Domain;

namespace Server.Api.Features.Instruction;

/// <summary>
/// Request to create a new instruction for an agent.
/// The payload is a raw JSON string that will be
/// deserialized based on the instruction type.
/// </summary>
public record CreateInstructionRequest(
  long AgentId,
  InstructionType Type,
  string PayloadJson);

/// <summary>
/// Request to create a new instruction for an
/// agent, without the agent ID (used in agent-specific endpoints).
/// </summary>
public record CreateAgentInstructionRequest(
  InstructionType Type,
  string PayloadJson);

/// <summary>
/// Request to create a shell command instruction for an agent.
/// </summary>
public record CreateShellCommandRequest(
  string Command,
  int Timeout = 5000);

/// <summary>
/// Request to create a GPO set instruction for an agent.
/// </summary>
public record CreateGpoSetRequest(
  string Name,
  string Value);

/// <summary>
/// Response model for an instruction, returned after creation or retrieval.
/// </summary>
public record InstructionResponse(
  long Id,
  long AgentId,
  InstructionType Type,
  string PayloadJson,
  InstructionState State,
  string? Output,
  string? Error,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt);

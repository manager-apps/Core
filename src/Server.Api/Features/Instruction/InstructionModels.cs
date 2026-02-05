using Server.Domain;

namespace Server.Api.Features.Instruction;

/// <summary>
/// Request to create a new instruction for an agent.
/// The payload is a raw JSON string that will be
/// deserialized based on the instruction type.
/// </summary>
/// <param name="AgentId"></param>
/// <param name="Type"></param>
/// <param name="PayloadJson"></param>
public record CreateInstructionRequest(
  long AgentId,
  InstructionType Type,
  string PayloadJson);

/// <summary>
/// Request to create a new instruction for an
/// agent, without the agent ID (used in agent-specific endpoints).
/// </summary>
/// <param name="Type"></param>
/// <param name="PayloadJson"></param>
public record CreateAgentInstructionRequest(
  InstructionType Type,
  string PayloadJson);

/// <summary>
/// Request to create a shell command instruction for an agent.
/// </summary>
/// <param name="Command"></param>
/// <param name="Timeout"></param>
public record CreateShellCommandRequest(
  string Command,
  int Timeout = 5000);

/// <summary>
/// Request to create a GPO set instruction for an agent.
/// </summary>
/// <param name="Name"></param>
/// <param name="Value"></param>
public record CreateGpoSetRequest(
  string Name,
  string Value);


/// <summary>
/// Response model for an instruction, returned after creation or retrieval.
/// </summary>
/// <param name="Id"></param>
/// <param name="AgentId"></param>
/// <param name="Type"></param>
/// <param name="PayloadJson"></param>
/// <param name="State"></param>
/// <param name="Output"></param>
/// <param name="Error"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
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

using Server.Domain;

namespace Server.Api.Features.Instruction;

public record CreateInstructionRequest(
  long AgentId,
  InstructionType Type,
  string PayloadJson);

public record CreateAgentInstructionRequest(
  InstructionType Type,
  string PayloadJson);

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

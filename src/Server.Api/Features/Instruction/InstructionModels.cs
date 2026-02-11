using Server.Domain;

namespace Server.Api.Features.Instruction;

public record CreateShellCommandRequest(
  string Command,
  int Timeout = 5000);

public record CreateGpoSetRequest(
  string Name,
  string Value);

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

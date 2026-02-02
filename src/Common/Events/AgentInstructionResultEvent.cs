using Common.Messages;

namespace Common.Events;

public record AgentInstructionResultEvent(
  InstructionResultMessage InstructionResult);

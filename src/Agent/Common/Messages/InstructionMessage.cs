namespace Common.Messages;

public record InstructionResultMessage(
  long AssociatedId,
  bool Success,
  string? Output,
  string? Error);

public record InstructionMessage(
  long AssociatedId,
  int Type,
  IReadOnlyDictionary<string, string> Payload);

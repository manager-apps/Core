namespace Common.Messages;

public record ReportMessageRequest (
  IEnumerable<MetricMessage> Metrics,
  IEnumerable<InstructionResultMessage> InstructionResults);

public record ReportMessageResponse (
  IEnumerable<InstructionMessage> Instructions);

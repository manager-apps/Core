namespace Agent.WindowsService.Common;

public record LoginMessageRequest(
  string AgentId,
  string ClientSecretKey);

public record LoginMessageResponse(
    string AuthToken,
    string RefreshToken);

public record MetricMessage(
  int Type,
  string Name,
  double Value,
  string Unit,
  DateTime TimestampUtc,
  Dictionary<string, object>? Metadata);

public record InstructionResultMessage(
  long AssociatedId,
  bool Success,
  string? Output,
  string? Error);

public record InstructionMessage(
  long AssociatedId,
  int Type,
  IReadOnlyDictionary<string, string> Payload);

public record ReportMessageRequest (
  IEnumerable<MetricMessage> Metrics,
  IEnumerable<InstructionResultMessage> InstructionResults);

public record ReportMessageResponse (
  IEnumerable<InstructionMessage> Instructions);

namespace Common.Messages;

public record MetricMessage(
  int Type,
  string Name,
  double Value,
  string Unit,
  DateTime TimestampUtc,
  Dictionary<string, object>? Metadata);



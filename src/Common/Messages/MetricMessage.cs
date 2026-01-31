namespace Common.Messages;

public record MetricMessage(
  string Type,
  string Name,
  double Value,
  string Unit,
  DateTime TimestampUtc,
  Dictionary<string, object>? Metadata);



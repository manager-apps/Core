namespace Agent.WindowsService.Domain;

public enum MetricType
{
  CpuUsage,
  MemoryUsage,
  DiskUsage,
  NetworkUsage
}

public class Metric
{
  public long Id { get; set; }

  /// <summary>
  /// Metric type
  /// </summary>
  public MetricType Type { get; set; }

  /// <summary>
  /// Metric name (for additional identification, e.g., disk name)
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Metric value
  /// </summary>
  public double Value { get; set; }

  /// <summary>
  /// Unit of measurement
  /// </summary>
  public string Unit { get; set; } = string.Empty;

  /// <summary>
  /// Timestamp of metric collection
  /// </summary>
  public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// Additional data (JSON)
  /// </summary>
  public Dictionary<string, object>? Metadata { get; set; }
}

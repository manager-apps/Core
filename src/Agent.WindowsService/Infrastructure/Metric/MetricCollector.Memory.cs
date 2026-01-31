using System.Diagnostics;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  private PerformanceCounter? _memoryCounter;

  /// <summary>
  /// Initialize Memory counter
  /// </summary>
  private void InitializeMemoryCounter()
  {
    try
    {
      _memoryCounter = new PerformanceCounter(
        MetricConfig.Memory.CounterCategoryName,
        MetricConfig.Memory.CounterName);
    }
    catch
    {
      // Performance counters may not be available
    }
  }

  /// <summary>
  /// Collect Memory usage metric in percentage
  /// </summary>
  private Domain.Metric CollectMemory()
  {
    var memoryUsage = _memoryCounter?.NextValue() ?? 0f;

    var metric = new Domain.Metric
    {
      Type = "memory_usage",
      Name = MetricConfig.Memory.Name,
      Unit = MetricConfig.Memory.Unit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(memoryUsage, 2),
    };

    return metric;
  }
}

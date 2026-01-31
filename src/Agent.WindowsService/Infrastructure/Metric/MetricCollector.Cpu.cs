using System.Diagnostics;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  private PerformanceCounter? _cpuCounter;

  /// <summary>
  /// Initialize CPU counter
  /// </summary>
  private void InitializeCpuCounter()
  {
    try
    {
      _cpuCounter = new PerformanceCounter(
        MetricConfig.Cpu.CounterCategoryName,
        MetricConfig.Cpu.CounterName,
        MetricConfig.Cpu.CounterInstanceName);
    }
    catch
    {
      // Performance counters may not be available
    }
  }

  /// <summary>
  /// Collect CPU usage metric in percentage
  /// </summary>
  private Domain.Metric CollectCpu()
  {
    var cpuUsage = _cpuCounter?.NextValue() ?? 0f;

    var metric = new Domain.Metric
    {
      Type = "cpu_usage",
      Name = MetricConfig.Cpu.Name,
      Unit = MetricConfig.Cpu.Unit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(cpuUsage, 2),
    };

    return metric;
  }
}

using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
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

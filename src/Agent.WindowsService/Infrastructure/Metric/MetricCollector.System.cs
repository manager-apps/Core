using System.Diagnostics;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  private readonly DateTime _startTime = DateTime.UtcNow;

  /// <summary>
  /// Collect system uptime
  /// </summary>
  private Domain.Metric CollectSystemUptime()
  {
    var uptime = (DateTime.UtcNow - _startTime).TotalHours;

    var metric = new Domain.Metric
    {
      Type = "system_uptime",
      Name = MetricConfig.System.UptimeName,
      Unit = MetricConfig.System.UptimeUnit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(uptime, 2),
    };

    return metric;
  }

  /// <summary>
  /// Collect available physical memory in MB
  /// </summary>
  private Domain.Metric CollectAvailablePhysicalMemory()
  {
    try
    {
      var availableMemory = GC.GetTotalMemory(false) / (1024.0 * 1024);

      var metric = new Domain.Metric
      {
        Type = "available_physical_memory",
        Name = MetricConfig.System.PhysicalMemoryName,
        Unit = MetricConfig.System.PhysicalMemoryUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = Math.Round(availableMemory, 2),
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "available_physical_memory",
        Name = MetricConfig.System.PhysicalMemoryName,
        Unit = MetricConfig.System.PhysicalMemoryUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect system timestamp for synchronization
  /// </summary>
  private Domain.Metric CollectSystemTimestamp()
  {
    var unixTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

    var metric = new Domain.Metric
    {
      Type = "system_timestamp",
      Name = MetricConfig.System.TimestampName,
      Unit = MetricConfig.System.TimestampUnit,
      TimestampUtc = DateTime.UtcNow,
      Value = unixTimestamp,
    };

    return metric;
  }
}

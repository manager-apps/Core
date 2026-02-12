using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  /// <summary>
  /// Collect Disk usage metric in percentage
  /// </summary>
  private Domain.Metric CollectDisk()
  {
    var driveInfos = DriveInfo.GetDrives()
        .Where(d => d is
        {
          IsReady: true,
          DriveType: DriveType.Fixed
        })
        .ToList();

    var totalSize = driveInfos.Sum(d => d.TotalSize);
    var totalFreeSpace = driveInfos.Sum(d => d.TotalFreeSpace);
    var usedSpace = totalSize - totalFreeSpace;

    var usedPercentage = totalSize > 0
      ? (double)usedSpace / totalSize * 100
      : 0;

    var metric = new Domain.Metric
    {
      Type = "disk_usage",
      Name = MetricConfig.Disk.Name,
      Unit = MetricConfig.Disk.Unit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(usedPercentage, 2),
    };

    return metric;
  }
}

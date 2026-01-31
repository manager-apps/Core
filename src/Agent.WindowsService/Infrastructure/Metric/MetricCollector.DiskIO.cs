using System.Diagnostics;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  private PerformanceCounter? _diskReadCounter;
  private PerformanceCounter? _diskWriteCounter;

  /// <summary>
  /// Initialize disk I/O counters
  /// </summary>
  private void InitializeDiskIoCounters()
  {
    try
    {
      _diskReadCounter = new PerformanceCounter(
        MetricConfig.DiskIO.CategoryName,
        MetricConfig.DiskIO.ReadBytesPerSecName,
        "_Total");

      _diskWriteCounter = new PerformanceCounter(
        MetricConfig.DiskIO.CategoryName,
        MetricConfig.DiskIO.WriteBytesPerSecName,
        "_Total");
    }
    catch
    {
      // Performance counters may not be available
    }
  }

  /// <summary>
  /// Collect per-drive disk space information
  /// </summary>
  private IEnumerable<Domain.Metric> CollectDiskSpacePerDrive()
  {
    var drives = DriveInfo.GetDrives()
      .Where(d => d is { IsReady: true, DriveType: DriveType.Fixed })
      .ToList();

    foreach (var drive in drives)
    {
      var usedSpace = drive.TotalSize - drive.TotalFreeSpace;
      var usedPercentage = drive.TotalSize > 0
        ? (double)usedSpace / drive.TotalSize * 100
        : 0;

      var metric = new Domain.Metric
      {
        Type = "disk_space",
        Name = drive.Name.TrimEnd('\\'),
        Unit = MetricConfig.DiskIO.SpaceUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = Math.Round(usedPercentage, 2),
        Metadata = new Dictionary<string, object>
        {
          { "totalBytes", drive.TotalSize },
          { "usedBytes", usedSpace },
          { "freeBytes", drive.TotalFreeSpace },
          { "availableBytes", drive.AvailableFreeSpace }
        }
      };

      yield return metric;
    }
  }

  /// <summary>
  /// Collect disk read speed in MB/s
  /// </summary>
  private Domain.Metric CollectDiskReadSpeed()
  {
    if (_diskReadCounter == null)
      InitializeDiskIoCounters();

    var bytesPerSecond = _diskReadCounter?.NextValue() ?? 0f;
    var mbPerSecond = bytesPerSecond / (1024.0 * 1024);

    var metric = new Domain.Metric
    {
      Type = "disk_read_speed",
      Name = MetricConfig.DiskIO.ReadSpeedName,
      Unit = MetricConfig.DiskIO.SpeedUnit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(mbPerSecond, 2),
    };

    return metric;
  }

  /// <summary>
  /// Collect disk write speed in MB/s
  /// </summary>
  private Domain.Metric CollectDiskWriteSpeed()
  {
    if (_diskWriteCounter == null)
      InitializeDiskIoCounters();

    var bytesPerSecond = _diskWriteCounter?.NextValue() ?? 0f;
    var mbPerSecond = bytesPerSecond / (1024.0 * 1024);

    var metric = new Domain.Metric
    {
      Type = "disk_write_speed",
      Name = MetricConfig.DiskIO.WriteSpeedName,
      Unit = MetricConfig.DiskIO.SpeedUnit,
      TimestampUtc = DateTime.UtcNow,
      Value = Math.Round(mbPerSecond, 2),
    };

    return metric;
  }
}

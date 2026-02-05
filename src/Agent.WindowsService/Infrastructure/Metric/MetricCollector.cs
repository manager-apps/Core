using Agent.WindowsService.Abstraction;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector : IMetricCollector
{
  public Task<IReadOnlyList<Domain.Metric>> CollectAsync(
    CancellationToken cancellationToken,
    IEnumerable<string> allowedCollectors)
  {
    var metrics = new List<Domain.Metric>();
    var allowed = allowedCollectors.ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (allowed.Count == 0)
      return Task.FromResult<IReadOnlyList<Domain.Metric>>(metrics);

    InitializeCpuCounter();
    InitializeMemoryCounter();
    InitializeDiskIoCounters();

    _cpuCounter?.NextValue();

    if (allowed.Contains("cpu_usage"))
      metrics.Add(CollectCpu());

    if (allowed.Contains("disk_usage"))
      metrics.Add(CollectDisk());

    if (allowed.Contains("memory_usage"))
      metrics.Add(CollectMemory());

    // metrics.Add(CollectDiskReadSpeed());
    // metrics.Add(CollectDiskWriteSpeed());
    // metrics.AddRange(CollectDiskSpacePerDrive());
    //
    //metrics.AddRange(CollectNetwork());
    //
    // metrics.Add(CollectProcessCount());
    // metrics.Add(CollectTotalHandleCount());
    // metrics.Add(CollectTotalThreadCount());
    // metrics.AddRange(CollectTopProcessesByCpu(5));
    // metrics.AddRange(CollectTopProcessesByMemory(5));
    //
    // metrics.Add(CollectSystemUptime());
    //metrics.Add(CollectAvailablePhysicalMemory());
    // metrics.Add(CollectSystemTimestamp());
    //
    // metrics.Add(CollectTotalTcpConnections());
    // metrics.Add(CollectEstablishedTcpConnections());
    // metrics.Add(CollectTimeWaitConnections());
    // metrics.Add(CollectCloseWaitConnections());
    // metrics.Add(CollectSynConnections());
    // metrics.Add(CollectTcpResets());
    // metrics.AddRange(CollectTcpConnectionsByState());
    //
    // metrics.Add(CollectApplicationErrors());
    // metrics.Add(CollectApplicationWarnings());
    // metrics.Add(CollectCriticalEvents());
    // metrics.Add(CollectInformationEvents());
    // metrics.Add(CollectTotalEventLogEntries());
    // metrics.Add(CollectHealthScore());

    return Task.FromResult<IReadOnlyList<Domain.Metric>>(metrics);
  }
}

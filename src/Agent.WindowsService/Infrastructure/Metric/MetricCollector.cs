using Agent.WindowsService.Abstraction;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector : IMetricCollector
{
  public Task<IReadOnlyList<Domain.Metric>> CollectAsync(CancellationToken cancellationToken)
  {
    var metrics = new List<Domain.Metric>();

    InitializeCpuCounter();
    InitializeMemoryCounter();
    InitializeDiskIoCounters();

    // _cpuCounter?.NextValue();

    metrics.Add(CollectCpu());
    metrics.Add(CollectDisk());
    metrics.Add(CollectMemory());

    metrics.Add(CollectDiskReadSpeed());
    metrics.Add(CollectDiskWriteSpeed());
    metrics.AddRange(CollectDiskSpacePerDrive());

    metrics.AddRange(CollectNetwork());

    metrics.Add(CollectProcessCount());
    metrics.Add(CollectTotalHandleCount());
    metrics.Add(CollectTotalThreadCount());
    metrics.AddRange(CollectTopProcessesByCpu(5));
    metrics.AddRange(CollectTopProcessesByMemory(5));

    metrics.Add(CollectSystemUptime());
    metrics.Add(CollectAvailablePhysicalMemory());
    metrics.Add(CollectSystemTimestamp());

    metrics.Add(CollectTotalTcpConnections());
    metrics.Add(CollectEstablishedTcpConnections());
    metrics.Add(CollectTimeWaitConnections());
    metrics.Add(CollectCloseWaitConnections());
    metrics.Add(CollectSynConnections());
    metrics.Add(CollectTcpResets());
    metrics.AddRange(CollectTcpConnectionsByState());

    metrics.Add(CollectApplicationErrors());
    metrics.Add(CollectApplicationWarnings());
    metrics.Add(CollectCriticalEvents());
    metrics.Add(CollectInformationEvents());
    metrics.Add(CollectTotalEventLogEntries());
    metrics.Add(CollectHealthScore());

    return Task.FromResult<IReadOnlyList<Domain.Metric>>(metrics);
  }
}

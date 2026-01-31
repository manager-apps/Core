using System.Diagnostics;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector : IMetricCollector
{
  private readonly PerformanceCounter? _cpuCounter
    = new(MetricConfig.Cpu.CounterCategoryName, MetricConfig.Cpu.CounterName, MetricConfig.Cpu.CounterInstanceName);

  private readonly PerformanceCounter? _memoryCounter
    = new(MetricConfig.Memory.CounterCategoryName, MetricConfig.Memory.CounterName);

  public Task<IReadOnlyList<Domain.Metric>> CollectAsync(CancellationToken cancellationToken)
  {
    var metrics = new List<Domain.Metric>();

    _cpuCounter?.NextValue();

    metrics.Add(CollectCpu());
    metrics.Add(CollectDisk());
    metrics.Add(CollectMemory());
    // metrics.AddRange(CollectNetwork());

    return Task.FromResult<IReadOnlyList<Domain.Metric>>(metrics);
  }
}

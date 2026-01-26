using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Abstraction;

public interface IMetricCollector
{
   /// <summary>
   /// Collect system metrics asynchronously
   /// </summary>
   Task<IReadOnlyList<Metric>> CollectAsync(CancellationToken cancellationToken);
}

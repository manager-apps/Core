using Common.Messages;

namespace MetricWorker.Interfaces;

public interface IMetricStorage
{
  Task StoreAsync(
    string agentName,
    MetricMessage metric,
    CancellationToken cancellationToken);
}

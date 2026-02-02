using Common.Messages;

namespace Server.MetricWorker.Interfaces;

public interface IMetricStorage
{
  Task StoreAsync(
    string agentName,
    MetricMessage metric,
    CancellationToken cancellationToken);
}

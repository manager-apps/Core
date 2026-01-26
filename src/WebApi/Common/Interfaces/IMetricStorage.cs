using Common.Messages;

namespace WebApi.Common.Interfaces;

public interface IMetricStorage
{
  /// <summary>
  /// Stores metrics for a given agent.
  /// </summary>
  /// <param name="agentName"></param>
  /// <param name="metrics"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task StoreMetricsAsync(
    string agentName,
    IEnumerable<MetricMessage> metrics,
    CancellationToken cancellationToken);
}

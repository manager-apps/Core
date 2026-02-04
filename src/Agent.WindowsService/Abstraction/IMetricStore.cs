namespace Agent.WindowsService.Abstraction;

// for the offline version, we need to store if we cant send to the server,
// then get them all and send them in batch when we have connection again.
// and remove them from the local store when sent successfully
public interface IMetricStore
{
  /// <summary>
  /// Stores the given metrics asynchronously.
  /// </summary>
  Task StoreAsync(
    IReadOnlyList<Domain.Metric> metrics,
    CancellationToken cancellationToken);

  /// <summary>
  /// Retrieves all stored metrics asynchronously.
  /// </summary>
  Task<IReadOnlyList<Domain.Metric>> GetAsync(
    CancellationToken cancellationToken,
    int limit = 20);

  /// <summary>
  /// Removes all stored metrics asynchronously.
  /// </summary>
  Task RemoveAsync(
    IEnumerable<long> ids,
    CancellationToken cancellationToken);
}

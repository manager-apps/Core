using Agent.WindowsService.Config;

namespace Agent.WindowsService.Abstraction;

public interface IServerClient
{
  /// <summary>
  /// Post request to server
  /// </summary>
  Task<TResponse?> Post<TResponse, TRequest>(
    string url,
    TRequest data,
    RequestMetadata metadata,
    CancellationToken cancellationToken);

  /// <summary>
  /// Get request from server
  /// </summary>
  Task<IEnumerable<TResponse>> Get<TResponse>(
    string url,
    RequestMetadata metadata,
    CancellationToken cancellationToken);
}

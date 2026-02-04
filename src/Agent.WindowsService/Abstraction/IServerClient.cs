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
}

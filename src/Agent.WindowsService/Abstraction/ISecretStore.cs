using System.Text;

namespace Agent.WindowsService.Abstraction;

public interface ISecretStore
{
  /// <summary>
  /// Sets the value for the specified key in the secret store.
  /// </summary>
  Task SetAsync(
    string key,
    ReadOnlyMemory<byte> value,
    CancellationToken cancellationToken);

  /// <summary>
  /// Retrieves the value for the specified key from the secret store as a string with the given encoding.
  /// </summary>
  Task<string> GetAsync(
    string key,
    Encoding encoding,
    CancellationToken cancellationToken);
}

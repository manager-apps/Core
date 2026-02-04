using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Store;

public class DpapiSecretStore : ISecretStore
{
  private readonly SemaphoreSlim _lock = new(1, 1);
  private Dictionary<string, byte[]> _cache = new();
  private bool _isLoaded;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
  };

  public async Task<string> GetAsync(string key, Encoding encoding, CancellationToken cancellationToken = default)
  {
    var data = await GetAsync(key, cancellationToken);
    if (data == null)
      return string.Empty;

    return encoding.GetString(data.Value.Span);
  }

  private async Task<ReadOnlyMemory<byte>?> GetAsync(
    string key, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      await EnsureLoadedAsync(cancellationToken);
      return _cache.GetValueOrDefault(key);
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task SetAsync(string key, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      await EnsureLoadedAsync(cancellationToken);
      _cache[key] = value.ToArray();
      await PersistAsync(cancellationToken);
    }
    finally
    {
      _lock.Release();
    }
  }

  private async Task EnsureLoadedAsync(CancellationToken ct)
  {
    if (_isLoaded)
      return;

    var path = PathConfig.SecretFilePath;
    if (!File.Exists(path))
    {
      await PersistAsync(ct);
      _isLoaded = true;
      return;
    }

    var encrypted = await File.ReadAllBytesAsync(path, ct);
    var decrypted = ProtectedData.Unprotect(encrypted, SecretConfig.Entropy, DataProtectionScope.CurrentUser);
    var deserialized = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(decrypted, JsonOptions);
    if (deserialized is not null)
      _cache = deserialized;

    _isLoaded = true;
  }

  private async Task PersistAsync(CancellationToken ct)
  {
    var path = PathConfig.SecretFilePath;

    var json = JsonSerializer.SerializeToUtf8Bytes(_cache, JsonOptions);
    var encrypted = ProtectedData.Protect(json, SecretConfig.Entropy, DataProtectionScope.CurrentUser);

    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    await File.WriteAllBytesAsync(path, encrypted, ct);
  }
}

using System.Text.Json;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Store;

public sealed class JsonConfigurationStore : IConfigurationStore
{
  private readonly SemaphoreSlim _lock = new(1, 1);

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
  };

  private Configuration? _cachedConfiguration;

  public async Task<Configuration> GetAsync(CancellationToken cancellationToken = default)
  {
    if (_cachedConfiguration is not null)
      return _cachedConfiguration;

    await _lock.WaitAsync(cancellationToken);
    try
    {
      if (_cachedConfiguration is not null)
        return _cachedConfiguration;

      var path = PathConfig.ConfigFilePath;
      if (!File.Exists(path))
      {
        var config = await CreateDefaultConfigFileAsync(cancellationToken);
        _cachedConfiguration = config;
        return config;
      }

      var json = await File.ReadAllTextAsync(path, cancellationToken);
      var configuration = JsonSerializer.Deserialize<Configuration>(json, JsonOptions);

      if (configuration is null)
        throw new InvalidDataException("Configuration file is invalid.");

      _cachedConfiguration = configuration;
      return configuration;
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task SaveAsync(Configuration configuration, CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      var path = PathConfig.ConfigFilePath;
      var json = JsonSerializer.Serialize(configuration, JsonOptions);

      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      await File.WriteAllTextAsync(path, json, cancellationToken);

      _cachedConfiguration = configuration;
    }
    finally
    {
      _lock.Release();
    }
  }

  private async Task<Configuration> CreateDefaultConfigFileAsync(CancellationToken cancellationToken)
  {
    var defaultConfig = new Configuration
    {
      AgentName = $"{Environment.MachineName}_{Guid.NewGuid()}",
      ServerCertificatedUrl = "http://localhost:5140",
    };

    var path = PathConfig.ConfigFilePath;
    var json = JsonSerializer.Serialize(defaultConfig, JsonOptions);

    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    await File.WriteAllTextAsync(path, json, cancellationToken);
    return defaultConfig;
  }
}

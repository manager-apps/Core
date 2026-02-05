using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.CommandLine;

/// <summary>
/// Handles command line operations for configuration and secrets management.
/// </summary>
public static class CommandLineHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Processes command line options and returns true if the service should run.
    /// </summary>
    public static async Task<bool> ProcessAsync(CommandLineOptions options)
    {
        if (options.SetVersion)
        {
          await SetVersionAsync(
            options.Version);

          return false;
        }

        if (options.InitConfig)
        {
            await InitializeConfigurationAsync(
              options.Version,
              options.ServerUrl,
              options.AgentName,
              options.Tag);
        }

        if (options.InitSecrets)
        {
            await InitializeSecretsAsync(options.ClientSecret);
        }

        return options.RunService;
    }

    private static async Task InitializeConfigurationAsync(
      string version,
      string? serverUrl,
      string? agentName,
      string? tag)
    {
      var config = new Configuration
      {
          Version = version,
          AgentName = agentName ?? $"{Environment.MachineName}_{Guid.NewGuid():N}",
          ServerUrl = serverUrl ?? "http://147.232.52.190:5000",
          Tag = tag ?? ""
      };
      var path = PathConfig.ConfigFilePath;
      var json = JsonSerializer.Serialize(config, JsonOptions);

      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      await File.WriteAllTextAsync(path, json);
    }

    private static async Task InitializeSecretsAsync(
      string? clientSecret)
    {
      var secrets = new Dictionary<string, byte[]>();
      if (!string.IsNullOrWhiteSpace(clientSecret))
      {
          secrets[SecretConfig.ClientSecretKey] = Encoding.UTF8.GetBytes(clientSecret);
      }

      var path = PathConfig.SecretFilePath;

      var json = JsonSerializer.SerializeToUtf8Bytes(secrets, JsonOptions);
      var encrypted = ProtectedData.Protect(json, SecretConfig.Entropy, DataProtectionScope.LocalMachine);

      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      await File.WriteAllBytesAsync(path, encrypted);
    }

    private static async Task SetVersionAsync(string version)
    {
      var path = PathConfig.ConfigFilePath;
      if (!File.Exists(path))
        return;

      var json = await File.ReadAllTextAsync(path);
      var config = JsonSerializer.Deserialize<Configuration>(json, JsonOptions);

      config?.Version = version;
      var updatedJson = JsonSerializer.Serialize(config, JsonOptions);
      await File.WriteAllTextAsync(path, updatedJson);
    }
}

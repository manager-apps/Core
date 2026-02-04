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
        if (options.ShowHelp)
        {
            CommandLineParser.PrintHelp();
            return false;
        }

        if (options.ShowConfig)
        {
            await ShowConfigurationAsync();
            return false;
        }

        if (options.InitConfig)
        {
            await InitializeConfigurationAsync(options.ServerUrl, options.AgentName, options.Tag);
        }

        if (options.InitSecrets)
        {
            await InitializeSecretsAsync(options.ClientSecret);
        }

        return options.RunService;
    }

    private static async Task ShowConfigurationAsync()
    {
        var configPath = PathConfig.ConfigFilePath;

        Console.WriteLine("=== Agent Configuration ===");
        Console.WriteLine($"Config Path: {configPath}");
        Console.WriteLine($"Secrets Path: {PathConfig.SecretFilePath}");
        Console.WriteLine($"Logs Path: {PathConfig.LogsFilePath}");
        Console.WriteLine();

        if (File.Exists(configPath))
        {
            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<Configuration>(json, JsonOptions);

            Console.WriteLine("Current Configuration:");
            Console.WriteLine($"  Agent Name: {config?.AgentName ?? "(not set)"}");
            Console.WriteLine($"  Server URL: {config?.ServerUrl ?? "(not set)"}");
        }
        else
        {
            Console.WriteLine("Configuration file not found. Use --init-config to create one.");
        }

        Console.WriteLine();
        Console.WriteLine($"Secrets file exists: {File.Exists(PathConfig.SecretFilePath)}");
    }

    private static async Task InitializeConfigurationAsync(
      string? serverUrl,
      string? agentName,
      string? tag)
    {
        Console.WriteLine("Initializing configuration...");

        var config = new Configuration
        {
            AgentName = agentName ?? $"{Environment.MachineName}_{Guid.NewGuid():N}",
            ServerUrl = serverUrl ?? "http://147.232.52.190:5000",
            Tag = tag
        };

        var path = PathConfig.ConfigFilePath;
        var json = JsonSerializer.Serialize(config, JsonOptions);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, json);

        Console.WriteLine($"Configuration saved to: {path}");
        Console.WriteLine($"  Agent Name: {config.AgentName}");
        Console.WriteLine($"  Server URL: {config.ServerUrl}");
    }

    private static async Task InitializeSecretsAsync(string? clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            Console.WriteLine("Warning: No client secret provided. Creating empty secrets store.");
        }
        else
        {
            Console.WriteLine("Initializing secrets...");
        }

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

        Console.WriteLine($"Secrets saved to: {path}");
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            Console.WriteLine($"  Client secret: [STORED SECURELY]");
        }
    }
}

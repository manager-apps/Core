using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using System.Text.Json;

namespace Agent.WindowsService.CommandLine;


public static class CommandLineHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<bool> ProcessAsync(CommandLineOptions options)
    {
        if (options.SetVersion)
        {
          await SetVersionAsync(options.Version);
          return false;
        }

        if (options.InitConfig)
        {
            await InitializeConfigurationAsync(
              options.Version,
              options.ServerUrl,
              options.AgentName,
              options.Tag,
              options.EnrollmentToken);
        }

        if (options.InitCertificate)
        {
            await InitializeCertificateAsync(
              options.CertificatePath,
              options.CertificatePassword);
        }

        return options.RunService;
    }

    private static async Task InitializeConfigurationAsync(
      string version,
      string? serverUrl,
      string? agentName,
      string? tag,
      string? enrollmentToken = null)
    {
      var config = new Configuration
      {
          Version = version,
          AgentName = agentName ?? $"{Environment.MachineName}_{Guid.NewGuid():N}",
          ServerUrl = serverUrl ?? "https://localhost:5141",
          Tag = tag ?? "",
          EnrollmentToken = enrollmentToken
      };
      var path = PathConfig.ConfigFilePath;
      var json = JsonSerializer.Serialize(config, JsonOptions);

      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      await File.WriteAllTextAsync(path, json);
    }

    private static async Task InitializeCertificateAsync(
      string? certificatePath,
      string? certificatePassword)
    {
      if (string.IsNullOrEmpty(certificatePath))
      {
        Console.WriteLine("No certificate path provided. Skipping certificate initialization.");
        return;
      }

      if (!File.Exists(certificatePath))
      {
        Console.WriteLine($"Certificate file not found: {certificatePath}");
        return;
      }

      try
      {
        var pfxBytes = await File.ReadAllBytesAsync(certificatePath);
        var cert = X509CertificateLoader.LoadPkcs12(
          pfxBytes,
          certificatePassword,
          X509KeyStorageFlags.Exportable);

        Console.WriteLine($"Certificate loaded: {cert.Subject}");
        Console.WriteLine($"Expires: {cert.NotAfter}");

        var certsDir = Path.Combine(PathConfig.BaseDirectory, "certs");
        Directory.CreateDirectory(certsDir);

        var encryptedPath = Path.Combine(certsDir, "agent.pfx.enc");
        var encryptedBytes = ProtectedData.Protect(
          pfxBytes,
          SecretConfig.CertificateEntropy,
          DataProtectionScope.LocalMachine);

        await File.WriteAllBytesAsync(encryptedPath, encryptedBytes);
        Console.WriteLine($"Certificate stored securely at: {encryptedPath}");

        cert.Dispose();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to initialize certificate: {ex.Message}");
      }
    }

    private static async Task SetVersionAsync(string version)
    {
      var path = PathConfig.ConfigFilePath;
      if (!File.Exists(path))
        return;

      var json = await File.ReadAllTextAsync(path);
      var config = JsonSerializer.Deserialize<Configuration>(json, JsonOptions);

      if (config is not null)
      {
        config.Version = version;
        var updatedJson = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(path, updatedJson);
      }
    }
}

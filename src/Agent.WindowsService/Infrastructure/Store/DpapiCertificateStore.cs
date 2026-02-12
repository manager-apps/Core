using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;

namespace Agent.WindowsService.Infrastructure.Store;

/// <summary>
/// DPAPI-protected certificate store for mTLS client certificates.
/// Stores certificates as encrypted PFX files using Windows DPAPI.
/// </summary>
public class DpapiCertificateStore(ILogger<DpapiCertificateStore> logger) : ICertificateStore
{
  private readonly SemaphoreSlim _lock = new(1, 1);
  private X509Certificate2? _cachedCertificate;
  private X509Certificate2? _cachedCaCertificate;

  private static readonly string CertificatePath = Path.Combine(PathConfig.BaseDirectory, "certs", "agent.pfx.enc");
  private static readonly string CaCertificatePath = Path.Combine(PathConfig.BaseDirectory, "certs", "ca.crt");

  public X509Certificate2? GetClientCertificate()
  {
    if (_cachedCertificate is not null && _cachedCertificate.NotAfter > DateTime.UtcNow)
      return _cachedCertificate;

    _lock.Wait();
    try
    {
      if (_cachedCertificate is not null && _cachedCertificate.NotAfter > DateTime.UtcNow)
        return _cachedCertificate;

      if (!File.Exists(CertificatePath))
      {
        logger.LogDebug("No client certificate found at {Path}", CertificatePath);
        return null;
      }

      var encryptedBytes = File.ReadAllBytes(CertificatePath);
      var pfxBytes = ProtectedData.Unprotect(
        encryptedBytes,
        SecretConfig.CertificateEntropy,
        DataProtectionScope.LocalMachine);

      _cachedCertificate = X509CertificateLoader.LoadPkcs12(
        pfxBytes,
        null,
        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

      logger.LogInformation(
        "Loaded client certificate. Subject: {Subject}, Expires: {Expiry}",
        _cachedCertificate.Subject,
        _cachedCertificate.NotAfter);

      return _cachedCertificate;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to load client certificate");
      return null;
    }
    finally
    {
      _lock.Release();
    }
  }


  public async Task StoreCertificateAsync(
    string certificatePem,
    RSA privateKey,
    CancellationToken cancellationToken)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      var cert = X509Certificate2.CreateFromPem(certificatePem);
      var certWithKey = cert.CopyWithPrivateKey(privateKey);

      var pfxBytes = certWithKey.Export(X509ContentType.Pfx);

      var encryptedBytes = ProtectedData.Protect(
        pfxBytes,
        SecretConfig.CertificateEntropy,
        DataProtectionScope.LocalMachine);

      var directory = Path.GetDirectoryName(CertificatePath)!;
      Directory.CreateDirectory(directory);

      await File.WriteAllBytesAsync(CertificatePath, encryptedBytes, cancellationToken);

      _cachedCertificate?.Dispose();
      _cachedCertificate = null;

      logger.LogInformation("Stored client certificate at {Path}", CertificatePath);

      cert.Dispose();
      certWithKey.Dispose();
    }
    finally
    {
      _lock.Release();
    }
  }

  public async Task StoreCaCertificateAsync(string caCertificatePem, CancellationToken cancellationToken)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      var directory = Path.GetDirectoryName(CaCertificatePath)!;
      Directory.CreateDirectory(directory);

      await File.WriteAllTextAsync(CaCertificatePath, caCertificatePem, cancellationToken);

      _cachedCaCertificate?.Dispose();
      _cachedCaCertificate = null;

      logger.LogInformation("Stored CA certificate at {Path}", CaCertificatePath);
    }
    finally
    {
      _lock.Release();
    }
  }

  public X509Certificate2? GetCaCertificate()
  {
    if (_cachedCaCertificate is not null)
      return _cachedCaCertificate;

    _lock.Wait();
    try
    {
      if (_cachedCaCertificate is not null)
        return _cachedCaCertificate;

      if (!File.Exists(CaCertificatePath))
      {
        logger.LogDebug("No CA certificate found at {Path}", CaCertificatePath);
        return null;
      }

      var pemContent = File.ReadAllText(CaCertificatePath);
      _cachedCaCertificate = X509Certificate2.CreateFromPem(pemContent);

      logger.LogInformation(
        "Loaded CA certificate. Subject: {Subject}",
        _cachedCaCertificate.Subject);

      return _cachedCaCertificate;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to load CA certificate");
      return null;
    }
    finally
    {
      _lock.Release();
    }
  }

  public bool HasValidCertificate()
  {
    var cert = GetClientCertificate();
    return cert is not null && cert.NotAfter > DateTime.UtcNow;
  }

  public DateTime? GetCertificateExpiry()
  {
    var cert = GetClientCertificate();
    return cert?.NotAfter;
  }

  public bool NeedsRenewal(int thresholdDays = 30)
  {
    var expiry = GetCertificateExpiry();
    if (expiry is null)
    {
      return true;
    }
    return expiry.Value <= DateTime.UtcNow.AddDays(thresholdDays);
  }

  public async Task DeleteCertificateAsync(CancellationToken cancellationToken)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      if (File.Exists(CertificatePath))
      {
        File.Delete(CertificatePath);
        logger.LogInformation("Deleted client certificate");
      }

      _cachedCertificate?.Dispose();
      _cachedCertificate = null;
    }
    finally
    {
      _lock.Release();
    }
  }
}

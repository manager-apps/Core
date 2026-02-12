using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Options;

namespace Server.Ingest.Infrastructure;

public sealed class CaAuthority : ICaAuthority
{
  private readonly ILogger<CaAuthority> _logger;
  private readonly X509Certificate2 _caCertificate;
  private readonly X509Certificate2 _caSigningCertificate;
  private readonly MtlsOptions _options;
  private readonly Lock _serialLock = new();
  private long _serialCounter;

  public CaAuthority(
      ILogger<CaAuthority> logger,
      IOptions<MtlsOptions> options)
  {
    _logger = logger;
    _options = options.Value;

    (_caCertificate, _caSigningCertificate) = LoadOrCreateCaCertificate();
    _serialCounter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    _logger.LogInformation(
        "Certificate Authority initialized. CA Subject: {Subject}, Expires: {Expiry}",
        _caCertificate.Subject,
        _caCertificate.NotAfter);
  }

  public Task<string> SignCertificateRequestAsync(
    string csrPem,
    string subjectName,
    int validityDays,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Signing certificate request for {SubjectName} with validity of {ValidityDays} days",
      subjectName,
      validityDays);

    cancellationToken.ThrowIfCancellationRequested();

    var csr = CertificateRequest.LoadSigningRequestPem(
      csrPem,
      HashAlgorithmName.SHA256,
      CertificateRequestLoadOptions.SkipSignatureValidation);

    var serialNumber = GenerateSerialNumber();
    var notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
    var notAfter = DateTimeOffset.UtcNow.AddDays(validityDays);

    using var rsaPrivateKey = _caSigningCertificate.GetRSAPrivateKey()
      ?? throw new InvalidOperationException("CA certificate does not have RSA private key");

    var subjectDn = new X500DistinguishedName($"CN={subjectName}");
    var certRequest = new CertificateRequest(
      subjectDn,
      csr.PublicKey,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1);

    AddCertificateExtensions(certRequest, csr.PublicKey);

    var signedCert = certRequest.Create(
      _caSigningCertificate,
      notBefore,
      notAfter,
      serialNumber);

    var certPem = signedCert.ExportCertificatePem();

    _logger.LogInformation(
      "Issued certificate for {SubjectName}. Serial: {Serial}, Expires: {Expiry}",
      subjectName,
      Convert.ToHexString(serialNumber),
      notAfter);

    return Task.FromResult(certPem);
  }

  public string GetCaCertificatePem()
    => _caCertificate.ExportCertificatePem();

  public bool ValidateCertificate(X509Certificate2 certificate)
  {
    _logger.LogInformation(
        "Validating client certificate. Subject: {Subject}, Issuer: {Issuer}, Thumbprint: {Thumbprint}",
        certificate.Subject,
        certificate.Issuer,
        certificate.Thumbprint);
    try
    {
      using var chain = new X509Chain();
      chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
      chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
      chain.ChainPolicy.ExtraStore.Add(_caCertificate);

      if (!chain.Build(certificate))
      {
        LogChainErrors(chain);
        return false;
      }

      var chainRoot = chain.ChainElements[^1].Certificate;
      if (!chainRoot.Thumbprint.Equals(_caCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogWarning("Certificate was not issued by this CA");
        return false;
      }

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Certificate validation error");
      return false;
    }
  }

  public CertificateInfo GetCertificateInfo(string certificatePem)
  {
    _logger.LogInformation("Extracting certificate info from PEM");

    var cert = X509Certificate2.CreateFromPem(certificatePem);
    return new CertificateInfoRecord(
      SerialNumber: cert.SerialNumber,
      Thumbprint: cert.Thumbprint,
      SubjectName: cert.GetNameInfo(X509NameType.SimpleName, false) ?? cert.Subject,
      IssuedAt: new DateTimeOffset(cert.NotBefore.ToUniversalTime(), TimeSpan.Zero),
      ExpiresAt: new DateTimeOffset(cert.NotAfter.ToUniversalTime(), TimeSpan.Zero));
  }

  #region Private Methods

  private void AddCertificateExtensions(CertificateRequest certRequest, PublicKey publicKey)
  {
    certRequest.CertificateExtensions.Add(
        new X509BasicConstraintsExtension(false, false, 0, true));

    certRequest.CertificateExtensions.Add(
        new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
            true));

    certRequest.CertificateExtensions.Add(
        new X509EnhancedKeyUsageExtension(
            [new Oid("1.3.6.1.5.5.7.3.2")],
            true));

    certRequest.CertificateExtensions.Add(
        new X509SubjectKeyIdentifierExtension(publicKey, false));

    var caSubjectKeyIdentifier = _caCertificate.Extensions
        .OfType<X509SubjectKeyIdentifierExtension>()
        .FirstOrDefault();

    if (caSubjectKeyIdentifier is not null)
    {
      certRequest.CertificateExtensions.Add(
          X509AuthorityKeyIdentifierExtension.CreateFromSubjectKeyIdentifier(caSubjectKeyIdentifier));
    }
  }

  private void LogChainErrors(X509Chain chain)
  {
    foreach (var status in chain.ChainStatus)
    {
      _logger.LogWarning(
          "Certificate validation failed: {Status} - {Information}",
          status.Status,
          status.StatusInformation);
    }
  }

  private byte[] GenerateSerialNumber()
  {
    lock (_serialLock)
    {
      _serialCounter++;
      var serial = new byte[16];
      var counterBytes = BitConverter.GetBytes(_serialCounter);
      var randomBytes = RandomNumberGenerator.GetBytes(8);
      Buffer.BlockCopy(counterBytes, 0, serial, 0, 8);
      Buffer.BlockCopy(randomBytes, 0, serial, 8, 8);
      serial[0] &= 0x7F;
      return serial;
    }
  }

  private (X509Certificate2 CaCert, X509Certificate2 SigningCert) LoadOrCreateCaCertificate()
  {
    if (string.IsNullOrEmpty(_options.CaPath))
    {
      throw new InvalidOperationException(
          "CA certificate path is not configured. Set 'Mtls:CaPath' in appsettings.json");
    }

    if (!File.Exists(_options.CaPath))
    {
      throw new FileNotFoundException(
          $"CA certificate file not found: {_options.CaPath}. " +
          "Generate certificates using Deploy/certs/Generate-Certificates.ps1");
    }

    var caCert = LoadCaCertificateFromFile();
    if (caCert is null)
    {
      throw new InvalidOperationException(
          $"Failed to load CA certificate from: {_options.CaPath}");
    }

    _logger.LogInformation("Loaded CA certificate from {Path}", _options.CaPath);
    return (caCert, caCert);
  }

  private X509Certificate2? LoadCaCertificateFromFile()
  {
    try
    {
      var caPath = _options.CaPath!;
      var keyPath = _options.CaKeyPath;

      if (string.IsNullOrEmpty(keyPath))
      {
        return X509CertificateLoader.LoadPkcs12FromFile(
          caPath,
          _options.CaPassword,
          X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
      }

      var certPem = File.ReadAllText(caPath);
      var keyPem = File.ReadAllText(keyPath);
      return X509Certificate2.CreateFromPem(certPem, keyPem);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load CA certificate from file");
        return null;
    }
  }

  #endregion
}

/// <summary>
/// Concrete implementation of CertificateInfo for infrastructure layer.
/// </summary>
internal sealed record CertificateInfoRecord(
    string SerialNumber,
    string Thumbprint,
    string SubjectName,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt
) : CertificateInfo(
  SerialNumber,
  Thumbprint,
  SubjectName,
  IssuedAt,
  ExpiresAt);

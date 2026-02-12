using System.Security.Cryptography.X509Certificates;

namespace Server.Ingest.Common.Interfaces;

public abstract record CertificateInfo(
  string SerialNumber,
  string Thumbprint,
  string SubjectName,
  DateTimeOffset IssuedAt,
  DateTimeOffset ExpiresAt);

public interface ICaAuthority
{
  /// <summary>
  /// Signs a Certificate Signing Request (CSR) and returns the signed certificate.
  /// </summary>
  Task<string> SignCertificateRequestAsync(
    string csrPem,
    string subjectName,
    int validityDays,
    CancellationToken cancellationToken);

  /// <summary>
  /// Gets the CA certificate chain in PEM format.
  /// </summary>
  string GetCaCertificatePem();

  /// <summary>
  /// Validates a client certificate against the CA.
  /// </summary>
  bool ValidateCertificate(X509Certificate2 certificate);

  /// <summary>
  /// Gets certificate information from PEM format.
  /// </summary>
  CertificateInfo GetCertificateInfo(string certificatePem);
}

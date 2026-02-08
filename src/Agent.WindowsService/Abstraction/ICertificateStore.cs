using System.Security.Cryptography.X509Certificates;

namespace Agent.WindowsService.Abstraction;

/// <summary>
/// Interface for managing client certificates for mTLS authentication.
/// </summary>
public interface ICertificateStore
{
  /// <summary>
  /// Gets the client certificate for mTLS authentication.
  /// </summary>
  /// <returns>The client certificate, or null if not available.</returns>
  X509Certificate2? GetClientCertificate();

  /// <summary>
  /// Stores a certificate from PEM format along with a private key.
  /// </summary>
  /// <param name="certificatePem">Certificate in PEM format.</param>
  /// <param name="privateKey">RSA private key.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task StoreCertificateAsync(
    string certificatePem,
    System.Security.Cryptography.RSA privateKey,
    CancellationToken cancellationToken);

  /// <summary>
  /// Stores the CA certificate for server validation.
  /// </summary>
  /// <param name="caCertificatePem">CA certificate in PEM format.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task StoreCaCertificateAsync(string caCertificatePem, CancellationToken cancellationToken);

  /// <summary>
  /// Gets the CA certificate for server validation.
  /// </summary>
  /// <returns>The CA certificate, or null if not available.</returns>
  X509Certificate2? GetCaCertificate();

  /// <summary>
  /// Checks if a valid client certificate exists.
  /// </summary>
  bool HasValidCertificate();

  /// <summary>
  /// Gets the expiry date of the current certificate.
  /// </summary>
  DateTime? GetCertificateExpiry();

  /// <summary>
  /// Checks if the certificate needs renewal.
  /// </summary>
  /// <param name="thresholdDays">Days before expiry to trigger renewal.</param>
  bool NeedsRenewal(int thresholdDays = 30);

  /// <summary>
  /// Deletes the stored certificate.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token.</param>
  Task DeleteCertificateAsync(CancellationToken cancellationToken);
}

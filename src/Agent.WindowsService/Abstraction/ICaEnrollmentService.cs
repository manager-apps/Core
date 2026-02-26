namespace Agent.WindowsService.Abstraction;

public interface ICaEnrollmentService
{
  /// <summary>
  /// Enrolls a new certificate using an enrollment token.
  /// </summary>
  Task<bool> EnrollWithTokenAsync(
    string serverUrl,
    string agentName,
    string sourceTag,
    string enrollmentToken,
    CancellationToken cancellationToken);

  /// <summary>
  /// Renews the current certificate using existing mTLS connection.
  /// </summary>
  Task<bool> RenewAsync(
    string serverUrl,
    CancellationToken cancellationToken);

  /// <summary>
  /// Checks if the current certificate is revoked.
  /// </summary>
  Task<bool> IsCertificateRevokedAsync(
    string serverUrl,
    CancellationToken cancellationToken);
}

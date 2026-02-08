namespace Server.Ingest.Features.Cert;

#region Enroll Models

/// <summary>
/// Response containing the issued certificate.
/// </summary>
public record CertEnrollResponse(
  string CertificatePem,
  string CaCertificatePem,
  DateTimeOffset ExpiresAt);

/// <summary>
/// Request for certificate enrollment using enrollment token.
/// </summary>
public sealed record TokenEnrollRequest(
  string AgentName,
  string CsrPem,
  string EnrollmentToken);

#endregion

#region Renew Models

/// <summary>
/// Request for certificate renewal.
/// </summary>
public sealed record CertRenewRequest(
  string CsrPem);

#endregion

#region Status Models

/// <summary>
/// Response containing certificate status information.
/// </summary>
public sealed record CertStatusResponse(
  string SerialNumber,
  string Thumbprint,
  string SubjectName,
  DateTimeOffset IssuedAt,
  DateTimeOffset ExpiresAt,
  bool IsValid,
  bool NeedsRenewal,
  DateTimeOffset? RevokedAt,
  string? RevocationReason);

#endregion


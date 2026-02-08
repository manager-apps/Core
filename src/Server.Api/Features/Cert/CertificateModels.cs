namespace Server.Api.Features.Cert;

/// <summary>
/// Request to create an enrollment token (admin only).
/// </summary>
public record CreateEnrollmentTokenRequest(
  string AgentName,
  int ValidityHours);

/// <summary>
/// Request to revoke all active certificates for an agent (admin only).
/// </summary>
public record RevokeRequest(
  string Reason);

/// <summary>
/// Response containing the created enrollment token.
/// </summary>
public record EnrollmentTokenResponse(
  string Token,
  string AgentName,
  DateTimeOffset ExpiresAt);

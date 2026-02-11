namespace Server.Api.Features.Cert;

public record CreateEnrollmentTokenRequest(
  string AgentName,
  int ValidityHours);

public record RevokeRequest(
  string Reason);

public record EnrollmentTokenResponse(
  string Token,
  string AgentName,
  DateTimeOffset ExpiresAt);

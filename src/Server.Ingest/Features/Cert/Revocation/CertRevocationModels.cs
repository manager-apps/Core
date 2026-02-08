namespace Server.Ingest.Features.Cert.Revocation;

/// <summary>
/// Response indicating whether a certificate is revoked.
/// </summary>
public sealed record CertRevocationResponse(
    bool IsRevoked,
    DateTimeOffset? RevokedAt,
    string? Reason);

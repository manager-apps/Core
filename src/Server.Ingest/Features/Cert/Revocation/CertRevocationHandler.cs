using Microsoft.EntityFrameworkCore;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Revocation;

/// <summary>
/// Handler for certificate revocation status checks.
/// </summary>
public interface ICertRevocationHandler
{
    /// <summary>
    /// Checks if a certificate with the given thumbprint is revoked.
    /// </summary>
    Task<Result<CertRevocationResponse>> CheckRevocationAsync(
        string thumbprint,
        CancellationToken cancellationToken);
}

internal sealed class CertRevocationHandler(
    ILogger<CertRevocationHandler> logger,
    AppDbContext dbContext) : ICertRevocationHandler
{
    public async Task<Result<CertRevocationResponse>> CheckRevocationAsync(
        string thumbprint,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking revocation status for certificate: {Thumbprint}", thumbprint);

        var certificate = await dbContext.Certificates
            .FirstOrDefaultAsync(c => c.Thumbprint == thumbprint, cancellationToken);

        if (certificate is null)
        {
            logger.LogWarning(
                "Certificate not found in database: {Thumbprint}. Assuming not revoked.",
                thumbprint);

            // If certificate not found, assume it's valid (not revoked)
            // This allows agents with valid CA-signed certs to continue working
            return new CertRevocationResponse(
                IsRevoked: false,
                RevokedAt: null,
                Reason: null);
        }

        var isRevoked = certificate.RevokedAt is not null;

        logger.LogInformation(
            "Certificate {Thumbprint} revocation status: {IsRevoked}",
            thumbprint,
            isRevoked);

        return new CertRevocationResponse(
            IsRevoked: isRevoked,
            RevokedAt: certificate.RevokedAt,
            Reason: certificate.RevocationReason);
    }
}

using System.Runtime.InteropServices.ComTypes;
using Microsoft.EntityFrameworkCore;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Revocation;

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
  AppDbContext dbContext
) : ICertRevocationHandler {
  public async Task<Result<CertRevocationResponse>> CheckRevocationAsync(
    string thumbprint,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Checking revocation status for certificate: {Thumbprint}", thumbprint);
    var certificate = await dbContext.Certificates
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.Thumbprint == thumbprint, cancellationToken);
    if (certificate is null)
    {
      logger.LogInformation("No certificate found for certificate: {Thumbprint}", thumbprint);
      return CertErrors.CertificateNotFound();
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

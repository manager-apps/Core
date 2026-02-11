using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Ingest.Common;
using Server.Ingest.Common.Options;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Status;

public interface ICertStatusHandler
{
  /// <summary>
  /// Gets the status of the active certificate for an agent.
  /// </summary>
  Task<Result<CertStatusResponse>> GetStatusAsync(
    ClaimsPrincipal claims,
    CancellationToken cancellationToken);
}

internal sealed class CertStatusHandler(
  ILogger<CertStatusHandler> logger,
  IOptions<MtlsOptions> mtlsOptions,
  AppDbContext dbContext) : ICertStatusHandler
{
  private readonly MtlsOptions _options = mtlsOptions.Value;
  public async Task<Result<CertStatusResponse>> GetStatusAsync(
    ClaimsPrincipal claims,
    CancellationToken cancellationToken)
  {
    logger.LogDebug("Getting certificate status for agent: {AgentName}", claims.Identity?.Name);

    var agentName = claims.Identity?.Name;
    if (string.IsNullOrEmpty(agentName))
    {
      logger.LogWarning("Unauthorized certificate status request with missing or invalid agent identity.");
      return CertErrors.AgentUnauthorized();
    }

    var agent = await dbContext.Agents
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
    if (agent is null)
    {
      logger.LogWarning("Certificate status request for non-existent agent: {AgentName}", agentName);
      return CertErrors.AgentNotFound();
    }

    var certificate = await dbContext.Certificates
        .AsNoTracking()
        .Where(c => c.AgentId == agent.Id && c.IsActive)
        .FirstOrDefaultAsync(cancellationToken);
    if (certificate is null)
    {
      logger.LogInformation("No active certificate found for agent: {AgentName}", agentName);
      return CertErrors.CertificateNotFound();
    }

    logger.LogDebug(@"
        Certificate status for agent {AgentName}: SerialNumber={SerialNumber},
        Thumbprint={Thumbprint}, SubjectName={SubjectName}, IssuedAt={IssuedAt},
        ExpiresAt={ExpiresAt}, IsValid={IsValid}, NeedsRenewal={NeedsRenewal},
        RevokedAt={RevokedAt}, RevocationReason={RevocationReason}",
        agentName,
        certificate.SerialNumber,
        certificate.Thumbprint,
        certificate.SubjectName,
        certificate.IssuedAt,
        certificate.ExpiresAt,
        certificate.IsValid(),
        certificate.NeedsRenewal(_options.RenewalThresholdDays),
        certificate.RevokedAt,
        certificate.RevocationReason);

    return new CertStatusResponse(
      SerialNumber: certificate.SerialNumber,
      Thumbprint: certificate.Thumbprint,
      SubjectName: certificate.SubjectName,
      IssuedAt: certificate.IssuedAt,
      ExpiresAt: certificate.ExpiresAt,
      IsValid: certificate.IsValid(),
      NeedsRenewal: certificate.NeedsRenewal(_options.RenewalThresholdDays),
      RevokedAt: certificate.RevokedAt,
      RevocationReason: certificate.RevocationReason);
  }
}

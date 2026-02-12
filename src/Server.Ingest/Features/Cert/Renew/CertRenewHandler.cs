using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Renew;

public interface ICertRenewHandler
{
  /// <summary>
  /// Renews an existing certificate for an authenticated agent.
  /// </summary>
  Task<Result<CertEnrollResponse>> RenewAsync(
    ClaimsPrincipal claims,
    CertRenewRequest request,
    CancellationToken cancellationToken);
}

internal sealed class CertRenewHandler(
  ILogger<CertRenewHandler> logger,
  ICertService certService,
  AppDbContext dbContext) : ICertRenewHandler
{
  public async Task<Result<CertEnrollResponse>> RenewAsync(
    ClaimsPrincipal claims,
    CertRenewRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Certificate renewal request for agent: {AgentName}", claims.Identity?.Name);

    var agentName = claims.Identity?.Name;
    if (string.IsNullOrEmpty(agentName))
    {
      logger.LogWarning("Unauthorized certificate renewal attempt with missing or invalid agent identity.");
      return CertErrors.AgentUnauthorized();
    }

    var agent = await dbContext.Agents
      .AsNoTracking()
      .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
    if (agent is null)
    {
      logger.LogWarning("Certificate renewal attempt for non-existent agent: {AgentName}", agentName);
      return CertErrors.AgentNotFound();
    }

    var currentCert = await dbContext.Certificates
      .AsNoTracking()
      .Where(c => c.AgentId == agent.Id && c.IsActive)
      .FirstOrDefaultAsync(cancellationToken);
    if (currentCert is null || currentCert.RevokedAt is not null)
    {
      logger.LogWarning("Certificate renewal attempt for agent '{AgentName}' without an active certificate.", agentName);
      return CertErrors.RenewalNotAllowed();
    }

    var issueCert = await certService.IssueCertificateAsync(
      agent.Id,
      agentName,
      request.CsrPem,
      cancellationToken);

    logger.LogInformation("Certificate renewal {Result} for agent: {AgentName}", issueCert.IsSuccess ? "succeeded" : "failed", agentName);

    return issueCert;
  }
}

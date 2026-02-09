using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Cert.Revoke;

internal interface ICertRevokeHandler
{
  /// <summary>
  /// Revokes all active certificates for an agent.
  /// </summary>
  Task<Result<bool>> HandleAsync(
    long agentId,
    string reason,
    CancellationToken cancellationToken);
}

internal sealed class CertRevokeHandler(
  ILogger<CertRevokeHandler> logger,
  AppDbContext dbContext) : ICertRevokeHandler
{
  public async Task<Result<bool>> HandleAsync(
    long agentId,
    string reason,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .FindAsync([agentId], cancellationToken);
    if (agent is null)
      return AgentErrors.NotFound();

    var certificates = await dbContext.Certificates
      .Where(c => c.AgentId == agent.Id &&
                  c.IsActive && c.RevokedAt == null)
      .ToListAsync(cancellationToken);
    if (certificates.Count == 0)
      return CertificateErrors.CertificateNotFound();

    foreach (var cert in certificates)
    {
      cert.Revoke(reason);
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Revoked {Count} certificate(s) for agent: {AgentName}. Reason: {Reason}",
      certificates.Count,
      agent.Name,
      reason);

    return true;
  }
}

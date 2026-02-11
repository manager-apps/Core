using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Cert.Revoke;

internal interface IAgentCertRevokeHandler
{
  /// <summary>
  /// Revokes all active certificates for an agent.
  /// </summary>
  Task<Result<bool>> HandleAsync(
    long agentId,
    RevokeRequest request,
    CancellationToken cancellationToken);
}

internal sealed class AgentCertRevokeHandler(
  ILogger<AgentCertRevokeHandler> logger,
  AppDbContext dbContext
) : IAgentCertRevokeHandler {
  public async Task<Result<bool>> HandleAsync(
    long agentId,
    RevokeRequest request,
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
      return CertificateErrors.NotFound();

    foreach (var cert in certificates)
    {
      cert.Revoke(request.Reason);
    }
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Revoked {Count} certificate(s) for agent: {AgentName}. Reason: {Reason}",
      certificates.Count,
      agent.Name,
      request.Reason);

    return true;
  }
}

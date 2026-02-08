using Microsoft.EntityFrameworkCore;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Renew;

/// <summary>
/// Handler for certificate renewal operations.
/// </summary>
public interface ICertRenewHandler
{
    /// <summary>
    /// Renews an existing certificate for an authenticated agent.
    /// </summary>
    Task<Result<CertEnrollResponse>> RenewAsync(
        string agentName,
        CertRenewRequest request,
        CancellationToken cancellationToken);
}

internal sealed class CertRenewHandler(
    ILogger<CertRenewHandler> logger,
    ICertService certService,
    AppDbContext dbContext) : ICertRenewHandler
{
    public async Task<Result<CertEnrollResponse>> RenewAsync(
        string agentName,
        CertRenewRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Certificate renewal request for agent: {AgentName}", agentName);

        var agent = await certService.FindAgentAsync(agentName, cancellationToken);
        if (agent is null)
            return CertErrors.AgentNotFound();

        var currentCert = await dbContext.Certificates
            .Where(c => c.AgentId == agent.Id && c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentCert is null || currentCert.RevokedAt is not null)
            return CertErrors.RenewalNotAllowed();

        return await certService.IssueCertificateAsync(
            agent.Id,
            agentName,
            request.CsrPem,
            cancellationToken);
    }
}

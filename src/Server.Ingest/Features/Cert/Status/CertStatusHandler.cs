using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Ingest.Common;
using Server.Ingest.Common.Options;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Status;

/// <summary>
/// Handler for certificate status queries.
/// </summary>
public interface ICertStatusHandler
{
    /// <summary>
    /// Gets the status of the active certificate for an agent.
    /// </summary>
    Task<Result<CertStatusResponse>> GetStatusAsync(
        string agentName,
        CancellationToken cancellationToken);
}

internal sealed class CertStatusHandler(
    ILogger<CertStatusHandler> logger,
    IOptions<MtlsOptions> mtlsOptions,
    AppDbContext dbContext) : ICertStatusHandler
{
    private readonly MtlsOptions _options = mtlsOptions.Value;

    public async Task<Result<CertStatusResponse>> GetStatusAsync(
        string agentName,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting certificate status for agent: {AgentName}", agentName);

        var agent = await dbContext.Agents
            .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);

        if (agent is null)
            return CertErrors.AgentNotFound();

        var certificate = await dbContext.Certificates
            .Where(c => c.AgentId == agent.Id && c.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (certificate is null)
            return CertErrors.CertificateNotFound();

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

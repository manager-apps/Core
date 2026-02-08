using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert.Enroll;

public interface ICertEnrollHandler
{
    /// <summary>
    /// Enrolls a new certificate using an enrollment token.
    /// </summary>
    Task<Result<CertEnrollResponse>> EnrollWithTokenAsync(
        TokenEnrollRequest request,
        CancellationToken cancellationToken);
}

internal sealed class CertEnrollHandler(
    ILogger<CertEnrollHandler> logger,
    ICertService certService,
    AppDbContext dbContext) : ICertEnrollHandler
{
    public async Task<Result<CertEnrollResponse>> EnrollWithTokenAsync(
        TokenEnrollRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Token-based enrollment for agent: {AgentName}", request.AgentName);

        var validToken = await certService.FindValidTokenAsync(
            request.AgentName,
            request.EnrollmentToken,
            cancellationToken);
        if (validToken is null)
            return CertErrors.InvalidEnrollmentToken();

        var agent = await certService.GetOrCreateAgentAsync(
          request.AgentName,
          cancellationToken);

        validToken.MarkAsUsed(agent.Id);

        var result = await certService.IssueCertificateAsync(
            agent.Id,
            request.AgentName,
            request.CsrPem,
            cancellationToken);
        if (result.IsSuccess)
            await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.Domain;
using Server.Ingest.Common;
using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Options;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Cert;


internal sealed class CertService(
    ILogger<CertService> logger,
    ICaAuthority caAuthority,
    IOptions<MtlsOptions> mtlsOptions,
    AppDbContext dbContext) : ICertService
{
  private readonly MtlsOptions _options = mtlsOptions.Value;

  public async Task<Result<CertEnrollResponse>> IssueCertificateAsync(
    long agentId,
    string agentName,
    string csrPem,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Issuing certificate for agent: {AgentName}", agentName);
    try
    {
      var certificatePem = await caAuthority.SignCertificateRequestAsync(
        csrPem,
        agentName,
        _options.CertificateValidityDays,
        cancellationToken);

      var certInfo = caAuthority.GetCertificateInfo(certificatePem);

      await DeactivatePreviousCertificatesAsync(agentId, cancellationToken);

      var certificate = Certificate.Create(
        agentId,
        certInfo.SerialNumber,
        certInfo.Thumbprint,
        certInfo.SubjectName,
        certInfo.IssuedAt,
        certInfo.ExpiresAt);

      dbContext.Certificates.Add(certificate);
      await dbContext.SaveChangesAsync(cancellationToken);

      logger.LogInformation(
        "Issued certificate for agent {AgentName}. Serial: {Serial}, Expires: {Expiry}",
        agentName,
        certInfo.SerialNumber,
        certInfo.ExpiresAt);

      return new CertEnrollResponse(
        CertificatePem: certificatePem,
        CaCertificatePem: caAuthority.GetCaCertificatePem(),
        ExpiresAt: certInfo.ExpiresAt);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to issue certificate for agent {AgentName}", agentName);
      return CertErrors.EnrollmentFailed(ex.Message);
    }
  }

    public async Task<bool> ValidateCertificateAsync(
      string thumbprint,
      string? expectedAgentName,
      CancellationToken cancellationToken)
    {
      logger.LogDebug("Validating certificate with thumbprint: {Thumbprint}", thumbprint);

      var certificate = await dbContext.Certificates
        .Include(c => c.Agent)
        .FirstOrDefaultAsync(c => c.Thumbprint == thumbprint && c.IsActive, cancellationToken);

      if (certificate is null)
      {
        logger.LogWarning("Certificate not found: {Thumbprint}", thumbprint);
        return false;
      }

      if (!certificate.IsValid())
      {
        logger.LogWarning("Certificate is not valid: {Thumbprint}", thumbprint);
        return false;
      }

      if (expectedAgentName is not null && certificate.Agent.Name != expectedAgentName)
      {
        logger.LogWarning(
          "Certificate agent mismatch. Expected: {Expected}, Actual: {Actual}",
          expectedAgentName,
          certificate.Agent.Name);
        return false;
      }

      return true;
    }

    private async Task DeactivatePreviousCertificatesAsync(
      long agentId,
      CancellationToken cancellationToken)
    {
      logger.LogDebug("Deactivating previous certificates for agent ID: {AgentId}", agentId);

      var previousCerts = await dbContext.Certificates
        .Where(c => c.AgentId == agentId && c.IsActive)
        .ToListAsync(cancellationToken);

      foreach (var cert in previousCerts)
      {
        cert.Deactivate();
      }
    }
}

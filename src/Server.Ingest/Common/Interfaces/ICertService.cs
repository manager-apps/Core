using Server.Domain;
using Server.Ingest.Common.Result;
using Server.Ingest.Features.Cert;

namespace Server.Ingest.Common.Interfaces;

public interface ICertService
{
  /// <summary>
  /// Issues a new certificate for the specified agent.
  /// </summary>
  Task<Result<CertEnrollResponse>> IssueCertificateAsync(
    long agentId,
    string agentName,
    string csrPem,
    CancellationToken cancellationToken);

  /// <summary>
  /// Validates a certificate thumbprint against the database.
  /// </summary>
  Task<bool> ValidateCertificateAsync(
    string thumbprint,
    string? expectedAgentName,
    CancellationToken cancellationToken);
}

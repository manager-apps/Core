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

  /// <summary>
  /// Finds an agent by name.
  /// </summary>
  Task<Agent?> FindAgentAsync(
    string agentName,
    CancellationToken cancellationToken);

  /// <summary>
  /// Gets or creates an agent with the specified name.
  /// </summary>
  Task<Agent> GetOrCreateAgentAsync(
    string agentName,
    CancellationToken cancellationToken);

  /// <summary>
  /// Finds a valid enrollment token for the specified agent.
  /// </summary>
  Task<EnrollmentToken?> FindValidTokenAsync(
    string agentName,
    string enrollmentToken,
    CancellationToken cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using Server.Domain;
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
  IDataHasher dataHasher,
  AppDbContext dbContext) : ICertEnrollHandler
{
  public async Task<Result<CertEnrollResponse>> EnrollWithTokenAsync(
    TokenEnrollRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Starting certificate enrollment for agent: {AgentName}", request.AgentName);

    var validToken = await FindValidTokenAsync(
        request.AgentName,
        request.EnrollmentToken,
        cancellationToken);
    if (validToken is null)
    {
      logger.LogWarning("Invalid enrollment token provided for agent: {AgentName}", request.AgentName);
      return CertErrors.InvalidEnrollmentToken();
    }

    var agent = await GetOrCreateAgentAsync(
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

    logger.LogInformation("Completed certificate enrollment for agent: {AgentName} with result: {Result}",
        request.AgentName,
        result.IsSuccess ? "Success" : $"Failure - {result.Error.Description}");

    return result;
  }

  private async Task<Agent> GetOrCreateAgentAsync(
    string agentName,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Looking up agent for enrollment: {AgentName}", agentName);

    var agent = await dbContext.Agents
      .AsNoTracking()
      .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
    if (agent is not null)
      return agent;

    logger.LogInformation("No existing agent found for enrollment. Creating new agent: {AgentName}", agentName);

    agent = Agent.Create(
      name: agentName,
      sourceTag: "empty_for_now");

    dbContext.Agents.Add(agent);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created new agent during enrollment: {AgentName}", agentName);
    return agent;
  }

  private async Task<EnrollmentToken?> FindValidTokenAsync(
    string agentName,
    string enrollmentToken,
    CancellationToken cancellationToken)
  {
    var tokens = await dbContext.EnrollmentTokens
      .Where(t => t.AgentName == agentName && !t.IsUsed)
      .ToListAsync(cancellationToken);

    return tokens.FirstOrDefault(token =>
      token.IsValid() && dataHasher.IsDataValid(
        enrollmentToken,
        token.TokenHash,
        token.TokenSalt));
  }
}

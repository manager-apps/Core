using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Result;
using Server.Api.Domain;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Auth;

internal interface IAgentAuthHandler
{
  /// <summary>
  /// Authenticate agent
  /// </summary>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<LoginMessageResponse>> AuthenticateAsync(
    LoginMessageRequest request,
    CancellationToken cancellationToken);
}

internal class AgentAuthHandler(
  ILogger<AgentAuthHandler> logger,
  IPasswordHasher passwordHasher,
  IJwtTokenProvider jwtTokenProvider,
  AppDbContext dbContext) : IAgentAuthHandler
{
  public async Task<Result<LoginMessageResponse>> AuthenticateAsync(
    LoginMessageRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Authenticating agent: {AgentName}", request.AgentName);

    var agent = await GetOrCreateAgentAsync(request, cancellationToken);
    if (!agent.CanAuthenticate())
    {
      logger.LogWarning("Authentication attempt for inactive agent: {AgentName}", agent.Name);
      return AgentErrors.Unauthorized();
    }

    if (!ValidatePassword(request, agent))
      return AgentErrors.Unauthorized();

    await UpdateLastSeenAsync(agent, cancellationToken);

    var token = jwtTokenProvider.GenerateTokenForAgent(agent.Name);

    logger.LogInformation("Authenticated agent: {AgentName}", agent.Name);

    return new LoginMessageResponse(
      AuthToken: token,
      RefreshToken: "refresh-token-placeholder");
  }

  private async Task<Domain.Agent> GetOrCreateAgentAsync(
    LoginMessageRequest request,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .FirstOrDefaultAsync(
        a => a.Name == request.AgentName,
        cancellationToken);

    if (agent is not null)
      return agent;

    var (hash, salt) = passwordHasher.CreatePasswordHash(request.SecretKey);
    agent = request.ToDomain(hash, salt);

    dbContext.Agents.Add(agent);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created new agent record: {AgentName}", request.AgentName);

    return agent;
  }

  /// <summary>
  /// Checks if the provided password matches the stored hash and salt.
  /// </summary>
  private bool ValidatePassword(LoginMessageRequest request, Domain.Agent agent)
    => passwordHasher.IsPasswordValid(
        password: request.SecretKey,
        storedHash: agent.SecretKeyHash,
        storedSalt: agent.SecretKeySalt);

  /// <summary>
  /// Updates the last seen timestamp of the agent.
  /// </summary>
  private async Task UpdateLastSeenAsync(
    Domain.Agent agent,
    CancellationToken cancellationToken)
  {
    agent.UpdateLastSeen();
    await dbContext.SaveChangesAsync(cancellationToken);
  }
}

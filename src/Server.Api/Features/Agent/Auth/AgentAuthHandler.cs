using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Auth;

internal interface IAgentAuthHandler
{
  /// <summary>
  /// Authenticate agent
  /// </summary>
  /// <param name="request"></param>
  /// <param name="tag">Agent tag from X-Tag header</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<AuthMessageResponse>> AuthenticateAsync(
    AuthMessageRequest request,
    string tag,
    CancellationToken cancellationToken);
}

internal class AgentAuthHandler(
  ILogger<AgentAuthHandler> logger,
  IDataHasher dataHasher,
  IJwtProvider jwtProvider,
  AppDbContext dbContext) : IAgentAuthHandler
{
  public async Task<Result<AuthMessageResponse>> AuthenticateAsync(
    AuthMessageRequest request,
    string tag,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Authenticating agent: {AgentName}", request.AgentName);

    var agent = await GetOrCreateAgentAsync(request, tag, cancellationToken);
    if (!agent.CanAuthenticate())
    {
      logger.LogWarning("Authentication attempt for inactive agent: {AgentName}", agent.Name);
      return AgentErrors.Unauthorized();
    }

    if (!dataHasher.IsDataValid(
          password: request.SecretKey,
          storedHash: agent.SecretKeyHash,
          storedSalt: agent.SecretKeySalt))
      return AgentErrors.Unauthorized();

    agent.UpdateLastSeen(tag);
    await dbContext.SaveChangesAsync(cancellationToken);

    var token = jwtProvider.GenerateTokenForAgent(agent.Name);

    logger.LogInformation("Authenticated agent: {AgentName}", agent.Name);

    return new AuthMessageResponse(
      AuthToken: token,
      RefreshToken: "refresh-token-placeholder");
  }

  private async Task<Server.Domain.Agent> GetOrCreateAgentAsync(
    AuthMessageRequest request,
    string sourceTag,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .FirstOrDefaultAsync(
        a => a.Name == request.AgentName,
        cancellationToken);

    if (agent is not null)
      return agent;

    var (hash, salt) = dataHasher.CreateDataHash(request.SecretKey);
    agent = request.ToDomain(hash, salt, sourceTag);

    dbContext.Agents.Add(agent);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created new agent record: {AgentName}", request.AgentName);

    return agent;
  }
}

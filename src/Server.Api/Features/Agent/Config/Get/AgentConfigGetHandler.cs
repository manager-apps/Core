using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Config;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Config.Get;

internal interface IAgentConfigGetHandler
{
  /// <summary>
  /// Handles the retrieval of an agent's config
  /// </summary>
  Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class AgentConfigGetHandler(
  ILogger<AgentConfigGetHandler> logger,
  AppDbContext dbContext
) : IAgentConfigGetHandler {
  public async Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Retrieving config for agent ID: {AgentId}", agentId);
    var agent = await dbContext.Agents
      .AsNoTracking()
      .Include(a => a.Config)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

    if (agent is null)
    {
      logger.LogWarning("Config retrieval attempt for non-existent agent ID: {AgentId}", agentId);
      return AgentErrors.NotFound();
    }

    if (agent.Config is null)
    {
      logger.LogInformation("No config found for agent ID: {AgentId}", agentId);
      return ConfigErrors.NotFound();
    }

    return agent.Config.ToResponse();
  }
}

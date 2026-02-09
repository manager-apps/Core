using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.GetAll;

internal interface IAgentGetAllHandler
{
  /// <summary>
  /// Get all agents
  /// </summary>
  Task<IEnumerable<AgentResponse>> HandleAsync(
    CancellationToken cancellationToken);
}

internal class AgentGetAllHandler (
  ILogger<AgentGetAllHandler> logger,
  AppDbContext dbContext
): IAgentGetAllHandler {
  public async Task<IEnumerable<AgentResponse>> HandleAsync(
    CancellationToken cancellationToken)
  {
    var agents = await dbContext.Agents
      .AsNoTracking()
      .ToListAsync(cancellationToken);

    logger.LogInformation("Retrieved {AgentCount} agents", agents.Count);

    return agents.Select(a => a.ToResponse());
  }
}

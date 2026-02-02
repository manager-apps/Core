using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.GetAll;

public interface IGetAllAgentsHandler
{
  /// <summary>
  /// Get all agents
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IEnumerable<AgentResponse>> HandleAsync(
    CancellationToken cancellationToken);
}

internal class GetAllAgentsHandler (
  ILogger<GetAllAgentsHandler> logger,
  AppDbContext dbContext
): IGetAllAgentsHandler
{
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

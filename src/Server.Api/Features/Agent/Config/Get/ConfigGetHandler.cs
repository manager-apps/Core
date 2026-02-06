using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Config;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Config.Get;

internal interface IConfigGetHandler
{
  /// <summary>
  /// Handles the retrieval of an agent's config
  /// </summary>
  Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class ConfigGetHandler(
  AppDbContext dbContext
) : IConfigGetHandler {
  public async Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .AsNoTracking()
      .Include(a => a.Config)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
    if (agent is null)
      return AgentErrors.NotFound();

    return agent.Config.ToResponse();
  }
}

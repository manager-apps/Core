using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.GetById;

internal interface IAgentGetByIdHandler
{
  /// <summary>
  /// Handles the retrieval of an agent by its ID
  /// </summary>
  Task<Result<AgentDetailResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class AgentGetByIdHandler (
  ILogger<AgentGetByIdHandler> logger,
  AppDbContext dbContext
) : IAgentGetByIdHandler {
  public async Task<Result<AgentDetailResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .AsNoTracking()
      .Include(a => a.Config)
      .Include(a => a.Hardware)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

    if (agent == null)
      return AgentErrors.NotFound();

    logger.LogInformation("Retrieved agent with ID {AgentId}", agentId);
    return agent.ToDetailResponse();
  }
}

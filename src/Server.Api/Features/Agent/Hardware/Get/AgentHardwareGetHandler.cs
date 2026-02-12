using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Hardware;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Hardware.Get;

internal interface IAgentHardwareGetHandler
{
  /// <summary>
  /// Handles the retrieval of an agent's hardware information
  /// </summary>
  Task<Result<HardwareResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class AgentHardwareGetHandler(
  ILogger<AgentHardwareGetHandler> logger,
  AppDbContext dbContext
) : IAgentHardwareGetHandler {
  public async Task<Result<HardwareResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Retrieving hardware information for agent ID: {AgentId}", agentId);

    var agent = await dbContext.Agents
      .AsNoTracking()
      .Include(a => a.Hardware)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

    if (agent is null)
    {
      logger.LogWarning("Hardware retrieval attempt for non-existent agent ID: {AgentId}", agentId);
      return AgentErrors.NotFound();
    }

    if (agent.Hardware is null)
    {
      logger.LogInformation("No hardware information found for agent ID: {AgentId}", agentId);
      return HardwareErrors.NotFound();
    }

    return agent.Hardware.ToResponse();
  }
}

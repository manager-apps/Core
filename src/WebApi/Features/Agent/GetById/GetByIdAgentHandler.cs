using WebApi.Common.Result;
using WebApi.Infrastructure;

namespace WebApi.Features.Agent.GetById;

internal interface IGetByIdAgentHandler
{
  Task<Result<AgentResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

public class GetByIdAgentHandler (
  ILogger<GetByIdAgentHandler> logger,
  AppDbContext dbContext
) : IGetByIdAgentHandler
{
  public async Task<Result<AgentResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .FindAsync([agentId], cancellationToken);
    if (agent == null)
    {
      logger.LogWarning("Agent with ID {AgentId} not found", agentId);
      return AgentErrors.NotFound();
    }

    logger.LogInformation("Retrieved agent with ID {AgentId}", agentId);
    return agent.ToResponse();
  }
}

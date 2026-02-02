using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Status;

internal  interface IUpdateStatusHandler
{
  /// <summary>
  /// Update agent state
  /// </summary>
  Task<Result<AgentResponse>> HandleAsync(
    long agentId,
    AgentUpdateStateRequest request,
    CancellationToken cancellationToken);
}

internal class UpdateStateHandler(
  ILogger<UpdateStateHandler> logger,
  AppDbContext dbContext
) : IUpdateStatusHandler
{
  public async Task<Result<AgentResponse>> HandleAsync(
    long agentId,
    AgentUpdateStateRequest request,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .FindAsync([agentId], cancellationToken);
    if(agent is null)
      return AgentErrors.NotFound();

    agent.Update(
      state: request.NewState);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Updated state for agent {AgentId} to {NewState}", agentId, request.NewState);
    return agent.ToResponse();
  }
}

using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.State.Update;

internal interface IStateUpdateHandler
{
  /// <summary>
  /// Handles the update of an agent's state
  /// </summary>
  Task<Result<AgentResponse>> HandleAsync(
    long agentId,
    AgentUpdateStateRequest request,
    CancellationToken cancellationToken);
}

internal class StateUpdateHandler(
  ILogger<StateUpdateHandler> logger,
  AppDbContext dbContext
) : IStateUpdateHandler {
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

    logger.LogInformation("Updated state for agent {AgentId} to {NewState}",
      agentId, request.NewState);
    return agent.ToResponse();
  }
}

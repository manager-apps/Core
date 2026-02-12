using System.Text.Json;
using Common;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Config;
using Server.Api.Infrastructure;
using Server.Domain;

namespace Server.Api.Features.Agent.Config.Update;

internal interface IAgentConfigUpdateHandler
{
  /// <summary>
  /// Handles the update of an agent's config and creates a new
  /// config instruction for the agent to sync with the new config
  /// </summary>
  Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    ConfigUpdateRequest request,
    CancellationToken cancellationToken);
}

internal class AgentConfigUpdateHandler(
  ILogger<AgentConfigUpdateHandler> logger,
  AppDbContext dbContext
) : IAgentConfigUpdateHandler {
  public async Task<Result<ConfigResponse>> HandleAsync(
    long agentId,
    ConfigUpdateRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Updating config for agent ID: {AgentId}", agentId);
    var agent = await dbContext.Agents
      .Include(a => a.Config)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

    if (agent is null)
    {
      logger.LogWarning("Config update attempt for non-existent agent ID: {AgentId}", agentId);
      return AgentErrors.NotFound();
    }

    if (agent.Config is null)
      return ConfigErrors.NotFound();

    agent.Config.Update(
      authenticationExitIntervalSeconds: request.AuthenticationExitIntervalSeconds,
      runningExitIntervalSeconds: request.RunningExitIntervalSeconds,
      executionExitIntervalSeconds: request.ExecutionExitIntervalSeconds,
      instructionsExecutionLimit: request.InstructionsExecutionLimit,
      instructionResultsSendLimit: request.InstructionResultsSendLimit,
      metricsSendLimit: request.MetricsSendLimit,
      allowedCollectors: request.AllowedCollectors,
      allowedInstructions: request.AllowedInstructions);

    var configMessage = agent.Config.ToMessage();
    var payload = new ConfigPayload(configMessage);
    var instruction = Server.Domain.Instruction.Create(
      agentId: agentId,
      type: InstructionType.Config,
      payloadJson: JsonSerializer
        .Serialize<InstructionPayload>(payload, JsonOptions.Default));

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Updated config for agent {AgentId} and created sync instruction {InstructionId}",
      agentId,
      instruction.Id);

    return agent.Config.ToResponse();
  }
}

using System.Security.Claims;
using System.Text.Json;
using Common;
using Common.Events;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Instruction;
using Server.Api.Infrastructure;
using Server.Domain;

namespace Server.Api.Features.Agent.Report;

internal interface IAgentReportHandler
{
  /// <summary>
  /// Handles reporting a message from an agent, and returns the instructions to execute.
  /// </summary>
  /// <param name="agent"></param>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<ReportMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    ReportMessageRequest request,
    CancellationToken cancellationToken);
}

internal class AgentReportHandler(
  ILogger<AgentReportHandler> logger,
  AppDbContext dbContext) : IAgentReportHandler
{
  public async Task<Result<ReportMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    ReportMessageRequest request,
    CancellationToken cancellationToken)
  {
    var agentInDb = await dbContext.Agents
      .FirstOrDefaultAsync(
        a => a.Name == agent.Identity!.Name!,
        cancellationToken: cancellationToken);
    if (agentInDb is null)
    {
      logger.LogWarning("Agent '{AgentName}' not found in database.", agent.Identity!.Name!);
      return AgentErrors.NotFound();
    }

    foreach (var metric in request.Metrics)
      dbContext.OutboxMessages.Add(OutboxMessage.Create(
        type: nameof(AgentMetricsEvent),
        payloadJson: JsonSerializer.Serialize(
          new AgentMetricsEvent(agentInDb.Name, metric),
          JsonOptions.Default)));

    foreach (var instructionResult in request.InstructionResults)
      dbContext.OutboxMessages.Add(OutboxMessage.Create(
        type: nameof(AgentInstructionResultEvent),
        payloadJson: JsonSerializer.Serialize(
          new AgentInstructionResultEvent(instructionResult),
          JsonOptions.Default)));

    await dbContext.SaveChangesAsync(cancellationToken);
    logger.LogInformation("Fetching pending instructions for agent '{AgentName}'.", agentInDb.Name);

    var pendingInstructions = await dbContext.Instructions
      .Where(i => i.State == InstructionState.Pending &&
                  i.AgentId == agentInDb.Id)
      .Take(10)
      .ToListAsync(cancellationToken: cancellationToken);

    foreach (var instruction in pendingInstructions)
      instruction.MarkAsDispatched();

    await dbContext.SaveChangesAsync(cancellationToken);

    var response = pendingInstructions
      .Select(i => new InstructionMessage(
        AssociatedId: i.Id,
        Type: (int)i.Type,
        Payload: InstructionUtils.DeserializePayload(i.Type, i.PayloadJson)))
      .ToList();

    logger.LogInformation("Returning {InstructionCount} instructions to agent.", response.Count);
    return new ReportMessageResponse(Instructions: response);
  }
}

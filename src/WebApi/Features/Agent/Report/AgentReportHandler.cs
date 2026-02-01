using System.Security.Claims;
using System.Text.Json;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Interfaces;
using WebApi.Common.Result;
using WebApi.Features.Instruction;
using WebApi.Infrastructure;

namespace WebApi.Features.Agent.Report;

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
  IMetricStorage metricStorage,
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


    var metricsToProcess = request.Metrics.ToList();
    if (metricsToProcess.Count > 0)
    {
      logger.LogInformation("Agent '{AgentName}' sent {MetricCount} metrics", agentInDb.Name, metricsToProcess.Count);

      await metricStorage.StoreMetricsAsync(
        agentInDb.Name,
        metricsToProcess,
        cancellationToken);
    }

    var instructionResults = request.InstructionResults;
    foreach (var instructionResult in instructionResults)
    {
      var instruction = await dbContext.Instructions
        .FirstOrDefaultAsync(
          i => i.Id == instructionResult.AssociatedId,
          cancellationToken: cancellationToken);
      if (instruction is null)
          continue;

      if (instructionResult.Success)
        instruction.MarkAsCompleted(instructionResult.Output!);
      else
        instruction.MarkAsFailed(instructionResult.Error!);
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    var pendingInstructions = await dbContext.Instructions
      .AsNoTracking()
      .Where(i => i.State == Domain.InstructionState.Pending &&
                  i.AgentId == agentInDb.Id)
      .Take(10)
      .Select(i => new InstructionMessage(
        AssociatedId: i.Id,
        Type: (int)i.Type,
        Payload: InstructionUtils.DeserializePayload(i.Type, i.PayloadJson)))
      .ToListAsync(cancellationToken: cancellationToken);

    logger.LogInformation("Returning {InstructionCount} instructions to agent.", pendingInstructions.Count);

    return new ReportMessageResponse(Instructions: pendingInstructions);
  }
}

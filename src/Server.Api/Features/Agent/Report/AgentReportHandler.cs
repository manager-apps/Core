using System.Text.Json;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Report;

internal interface IAgentReportHandler
{
  /// <summary>
  /// Handles reporting a message from an agent, and returns the instructions to execute.
  /// </summary>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<ReportMessageResponse>> HandleAsync(
    ReportMessageRequest request,
    CancellationToken cancellationToken);
}

internal class AgentReportHandler(
  ILogger<AgentReportHandler> logger,
  AppDbContext dbContext) : IAgentReportHandler
{
  public async Task<Result<ReportMessageResponse>> HandleAsync(
    ReportMessageRequest request,
    CancellationToken cancellationToken)
  {
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
      .Where(i => i.State == Domain.InstructionState.Pending)
      .Take(10)
      .Select(i => new InstructionMessage(
        AssociatedId: i.Id,
        Type: (int)i.Type,
        Payload: JsonSerializer.Deserialize<Dictionary<string, string>>(i.Payload)!))
      .ToListAsync(cancellationToken: cancellationToken);

    logger.LogInformation("Returning {InstructionCount} instructions to agent.", pendingInstructions.Count);

    return new ReportMessageResponse(Instructions: pendingInstructions);
  }
}

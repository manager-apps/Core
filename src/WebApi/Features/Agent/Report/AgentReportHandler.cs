using System.Security.Claims;
using System.Text.Json;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Interfaces;
using WebApi.Common.Result;
using WebApi.Infrastructure;

namespace WebApi.Features.Agent.Report;

internal interface IAgentReportHandler
{
  /// <summary>
  /// Handles reporting a message from an agent, and returns the instructions to execute.
  /// </summary>
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
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    WriteIndented = false
  };

  public async Task<Result<ReportMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    ReportMessageRequest request,
    CancellationToken cancellationToken)
  {
    var metricsToProcess = request.Metrics.ToList();
    if (metricsToProcess.Count > 0)
    {
      logger.LogInformation("Agent '{AgentName}' sent {MetricCount} metrics", agent.Identity!.Name!, metricsToProcess.Count);

      await metricStorage.StoreMetricsAsync(
        agent.Identity!.Name!,
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
      .Where(i => i.State == Domain.InstructionState.Pending)
      .Take(10)
      .Select(i => new InstructionMessage(
        AssociatedId: i.Id,
        Type: (int)i.Type,
        Payload: DeserializePayload(i.Type, i.Payload)))
      .ToListAsync(cancellationToken: cancellationToken);

    logger.LogInformation("Returning {InstructionCount} instructions to agent.", pendingInstructions.Count);

    return new ReportMessageResponse(Instructions: pendingInstructions);
  }

  /// <summary>
  /// Deserializes JSON payload to typed InstructionPayload.
  /// </summary>
  private static InstructionPayload DeserializePayload(Domain.InstructionType type, string json)
  {
    return type switch
    {
      Domain.InstructionType.ShellCommand =>
        JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
          ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),

      Domain.InstructionType.GpoSet =>
        JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
          ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),
      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }
}

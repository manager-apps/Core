using System.Security.Claims;
using System.Text.Json;
using Common;
using Common.Events;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Report.Create;

internal interface IReportCreateHandler
{
  /// <summary>
  /// Handles incoming report messages from agents, processes metrics and instruction results,
  /// </summary>
  Task<Result<ReportMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    ReportMessageRequest request,
    CancellationToken cancellationToken);
}

internal class ReportCreateHandler(
  ILogger<ReportCreateHandler> logger,
  AppDbContext dbContext
) : IReportCreateHandler
{
  public async Task<Result<ReportMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    ReportMessageRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Received report message from agent: {AgentName}", agent.Identity?.Name);

    var agentName = agent.Identity?.Name!;
    if (string.IsNullOrEmpty(agentName))
    {
      logger.LogWarning("Unauthorized report message attempt with missing or invalid agent identity.");
      return ReportErrors.AgentUnauthorized();
    }

    var agentInDb = await dbContext.Agents
      .AsNoTracking()
      .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
    if (agentInDb is null)
    {
      logger.LogWarning("Report message attempt for non-existent agent: {AgentName}", agentName);
      return ReportErrors.AgentNotFound();
    }

    foreach (var metric in request.Metrics)
      dbContext.OutboxMessages.Add(OutboxMessage.Create(
        type: nameof(AgentMetricsEvent),
        payloadJson: JsonSerializer.Serialize(
          new AgentMetricsEvent(agentName, metric),
          JsonOptions.Default)));

    foreach (var instructionResult in request.InstructionResults)
      dbContext.OutboxMessages.Add(OutboxMessage.Create(
        type: nameof(AgentInstructionResultEvent),
        payloadJson: JsonSerializer.Serialize(
          new AgentInstructionResultEvent(instructionResult),
          JsonOptions.Default)));

    await dbContext.SaveChangesAsync(cancellationToken);
    logger.LogInformation("Fetching pending instructions for agent '{AgentName}'.", agentName);

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
        Payload: InstructionUtils.DeserializePayload(
          i.Type, i.PayloadJson)))
      .ToList();

    logger.LogInformation("Returning {InstructionCount} instructions to agent.", response.Count);
    return new ReportMessageResponse(Instructions: response);
  }
}

static class InstructionUtils
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    WriteIndented = false
  };

  public static InstructionPayload DeserializePayload(InstructionType type, string json)
  {
    return type switch
    {
      InstructionType.Shell =>
        JsonSerializer.Deserialize<ShellCommandPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ShellCommandPayload)}"),

      InstructionType.Gpo =>
        JsonSerializer.Deserialize<GpoSetPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(GpoSetPayload)}"),

      InstructionType.Config =>
        JsonSerializer.Deserialize<ConfigPayload>(json, JsonOptions)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ConfigPayload)}"),

      _ => throw new ArgumentException($"Unknown instruction type: {type}")
    };
  }
}

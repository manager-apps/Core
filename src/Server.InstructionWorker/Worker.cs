using System.Text.Json;
using Common;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.InstructionWorker.Infrastructure;

namespace Server.InstructionWorker;

public class Worker(
  IServiceScopeFactory scopeFactory,
  ILogger<Worker> logger) : BackgroundService
{
  private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
  private const int BatchSize = 20;
  private const int MaxRetryCount = 3;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("InstructionWorker started");

    while (!stoppingToken.IsCancellationRequested)
    {
      var processedCount = await ProcessBatchAsync(stoppingToken);
      if (processedCount == 0)
        await Task.Delay(PollingInterval, stoppingToken);
    }

    logger.LogInformation("InstructionWorker stopped");
  }

  private async Task<int> ProcessBatchAsync(CancellationToken ct)
  {
    try
    {
      await using var scope = scopeFactory.CreateAsyncScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

      var messages = await FetchPendingMessagesAsync(dbContext, ct);
      if (messages.Count == 0)
        return 0;

      var successCount = 0;
      foreach (var message in messages)
      {
        if (await TryProcessMessageAsync(message, dbContext, ct))
          successCount++;
      }

      await dbContext.SaveChangesAsync(ct);

      logger.LogInformation("Processed {SuccessCount}/{TotalCount} instruction messages",
        successCount, messages.Count);

      return messages.Count;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      logger.LogError(ex, "Failed to process instruction batch");
      return 0;
    }
  }

  private static async Task<List<OutboxMessage>> FetchPendingMessagesAsync(
    AppDbContext dbContext,
    CancellationToken ct) => await dbContext.OutboxMessages
      .FromSqlRaw(
        """
          SELECT * FROM "OutboxMessages"
          WHERE "State" = {0} AND "RetryCount" < {1} AND "Type" = {2}
          ORDER BY "OccurredAt"
          LIMIT {3}
          FOR UPDATE SKIP LOCKED
        """,
        (int)OutboxMessageState.InProcess,
        MaxRetryCount,
        nameof(AgentInstructionResultEvent),
        BatchSize)
      .ToListAsync(ct);

  private async Task<bool> TryProcessMessageAsync(
    OutboxMessage message,
    AppDbContext dbContext,
    CancellationToken ct)
  {
    try
    {
      var @event = JsonSerializer.Deserialize<AgentInstructionResultEvent>(
        message.PayloadJson,
        JsonOptions.Default);

      if (@event is null)
      {
        message.MarkAsFailed("Failed to deserialize AgentInstructionResultEvent");
        return false;
      }

      var instruction = await dbContext.Instructions
        .FirstOrDefaultAsync(i => i.Id == @event.InstructionResult.AssociatedId, ct);

      if (instruction is null)
      {
        logger.LogWarning("Instruction {InstructionId} not found",
          @event.InstructionResult.AssociatedId);
        message.MarkAsFailed($"Instruction {@event.InstructionResult.AssociatedId} not found");
        return false;
      }

      if (@event.InstructionResult.Success)
        instruction.MarkAsCompleted(@event.InstructionResult.Output ?? string.Empty);
      else
        instruction.MarkAsFailed(@event.InstructionResult.Error ?? "Unknown error");

      message.MarkAsProcessed();

      logger.LogDebug("Processed instruction {InstructionId} result: {Success}",
        instruction.Id, @event.InstructionResult.Success);

      return true;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      logger.LogWarning(ex, "Failed to process instruction message, retry {RetryCount}",
        message.RetryCount + 1);
      message.MarkAsFailed(ex.Message);
      return false;
    }
  }
}

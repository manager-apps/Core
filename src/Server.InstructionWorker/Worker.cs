using Common.Events;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.InstructionWorker.Infrastructure;

namespace Server.InstructionWorker;

public class Worker(
  IServiceScopeFactory scopeFactory,
  IInstructionResultProcessor processor,
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
        if (await processor.ProcessAsync(message, dbContext, ct))
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
}

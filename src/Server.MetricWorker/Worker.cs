using System.Text.Json;
using Common;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.MetricWorker.Infrastructure;
using Server.MetricWorker.Interfaces;

namespace Server.MetricWorker;

public class Worker(
  IServiceScopeFactory scopeFactory,
  ILogger<Worker> logger) : BackgroundService
{
  private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
  private const int BatchSize = 20;
  private const int MaxRetryCount = 3;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("MetricWorker started");

    while (!stoppingToken.IsCancellationRequested)
    {
      var processedCount = await ProcessBatchAsync(stoppingToken);
      if (processedCount == 0)
        await Task.Delay(PollingInterval, stoppingToken);
    }

    logger.LogInformation("MetricWorker stopped");
  }

  private async Task<int> ProcessBatchAsync(CancellationToken ct)
  {
    try
    {
      await using var scope = scopeFactory.CreateAsyncScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var metricStorage = scope.ServiceProvider.GetRequiredService<IMetricStorage>();

      var messages = await FetchPendingMessagesAsync(dbContext, ct);
      if (messages.Count == 0)
        return 0;

      var successCount = 0;
      foreach (var message in messages)
      {
        if (await TryProcessMessageAsync(message, dbContext, metricStorage, ct))
          successCount++;
      }

      await dbContext.SaveChangesAsync(ct);

      logger.LogInformation("Processed {SuccessCount}/{TotalCount} metric messages",
        successCount, messages.Count);

      return messages.Count;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      logger.LogError(ex, "Failed to process metric batch");
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
        nameof(AgentMetricsEvent),
        BatchSize)
      .ToListAsync(ct);

  private async Task<bool> TryProcessMessageAsync(
    OutboxMessage message,
    AppDbContext dbContext,
    IMetricStorage metricStorage,
    CancellationToken ct)
  {
    try
    {
      var @event = JsonSerializer.Deserialize<AgentMetricsEvent>(
        message.PayloadJson,
        JsonOptions.Default);

      if (@event is null)
      {
        message.MarkAsFailed("Failed to deserialize AgentMetricsEvent");
        return false;
      }

      var agent = await dbContext.Agents
        .AsNoTracking()
        .FirstOrDefaultAsync(a => a.Name == @event.AgentName, ct);

      if (agent is null)
      {
        logger.LogWarning("Agent {AgentName} not found", @event.AgentName);
        message.MarkAsFailed($"Agent {@event.AgentName} not found");
        return false;
      }

      await metricStorage.StoreAsync(@event.AgentName, @event.Metric, ct);
      message.MarkAsProcessed();

      logger.LogDebug("Processed metric {MetricType} for agent {AgentName}",
        @event.Metric.Type, @event.AgentName);

      return true;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      logger.LogWarning(ex, "Failed to process metric message, retry {RetryCount}",
        message.RetryCount + 1);
      message.MarkAsFailed(ex.Message);
      return false;
    }
  }
}

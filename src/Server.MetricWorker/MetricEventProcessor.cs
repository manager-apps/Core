using System.Text.Json;
using Common;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Server.Domain;
using Server.MetricWorker.Infrastructure;
using Server.MetricWorker.Interfaces;

namespace Server.MetricWorker;

public interface IMetricEventProcessor
{
  Task<bool> ProcessAsync(
    OutboxMessage message,
    AppDbContext dbContext,
    IMetricStorage metricStorage,
    CancellationToken ct);
}

internal sealed class MetricEventProcessor(
  HybridCache cache,
  ILogger<MetricEventProcessor> logger) : IMetricEventProcessor
{
  private static string AgentCacheKey(string name) => $"agent:{name}";

  public async Task<bool> ProcessAsync(
    OutboxMessage message,
    AppDbContext dbContext,
    IMetricStorage metricStorage,
    CancellationToken ct)
  {
    try
    {
      var @event = JsonSerializer.Deserialize<AgentMetricsEvent>(
        message.PayloadJson, JsonOptions.Default);

      if (@event is null)
      {
        message.MarkAsFailed("Failed to deserialize AgentMetricsEvent");
        return false;
      }

      var agent = await cache.GetOrCreateAsync(
        AgentCacheKey(@event.AgentName),
        async token => await dbContext.Agents
          .AsNoTracking()
          .FirstOrDefaultAsync(a => a.Name == @event.AgentName, token),
        cancellationToken: ct);

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
      logger.LogWarning(ex, "Failed to process metric message");
      message.MarkAsFailed(ex.Message);
      return false;
    }
  }
}

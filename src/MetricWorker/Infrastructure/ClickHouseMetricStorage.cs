using System.Text.Json;
using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Utility;
using Common;
using Common.Messages;
using MetricWorker.Interfaces;

namespace MetricWorker.Infrastructure;

public class ClickHouseMetricStorage(
  ILogger<ClickHouseMetricStorage> logger,
  ClickHouseConnection connection) : IMetricStorage
{
  public async Task StoreAsync(
    string agentName,
    IEnumerable<MetricMessage> metrics,
    CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.CommandText =
    """
      INSERT INTO agent_metrics (AgentName, Type, Name, Value, Unit, TimestampUtc, Metadata)
      VALUES (@AgentName, @Type, @Name, @Value, @Unit, @TimestampUtc, @Metadata)
    """;

    foreach (var metric in metrics)
    {
      command.Parameters.Clear();
      command.AddParameter("AgentName", agentName);
      command.AddParameter("Type", metric.Type);
      command.AddParameter("Name", metric.Name);
      command.AddParameter("Value", metric.Value);
      command.AddParameter("Unit", metric.Unit);
      command.AddParameter("TimestampUtc", metric.TimestampUtc);
      command.AddParameter("Metadata", JsonSerializer.Serialize(metric.Metadata ?? new Dictionary<string, object>(), JsonOptions.Default));

      await command.ExecuteNonQueryAsync(cancellationToken);
    }

    logger.LogInformation("Stored {Count} metrics to ClickHouse", metrics.Count());
  }

  public async Task StoreAsync(string agentName, MetricMessage metric, CancellationToken cancellationToken)
  {
    await using var command = connection.CreateCommand();
    command.CommandText =
    """
      INSERT INTO agent_metrics (AgentName, Type, Name, Value, Unit, TimestampUtc, Metadata)
      VALUES (@AgentName, @Type, @Name, @Value, @Unit, @TimestampUtc, @Metadata)
    """;

    command.AddParameter("AgentName", agentName);
    command.AddParameter("Type", metric.Type);
    command.AddParameter("Name", metric.Name);
    command.AddParameter("Value", metric.Value);
    command.AddParameter("Unit", metric.Unit);
    command.AddParameter("TimestampUtc", metric.TimestampUtc);
    command.AddParameter("Metadata", JsonSerializer.Serialize(metric.Metadata ?? new Dictionary<string, object>(), JsonOptions.Default));

    await command.ExecuteNonQueryAsync(cancellationToken);
    logger.LogInformation("Stored 1 metric to ClickHouse");
  }
}

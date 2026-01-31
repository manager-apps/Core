using ClickHouse.Driver.ADO;
using ClickHouse.Driver.Utility;
using Common.Messages;
using WebApi.Common.Interfaces;

namespace WebApi.Infrastructure;

public class ClickHouseMetricStorage(
  ClickHouseConnection connection) : IMetricStorage
{
  public async Task StoreMetricsAsync(
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
      command.AddParameter("Metadata", System.Text.Json.JsonSerializer.Serialize(metric.Metadata
        ?? new Dictionary<string, object>()));

      await command.ExecuteNonQueryAsync(cancellationToken);
    }
  }
}

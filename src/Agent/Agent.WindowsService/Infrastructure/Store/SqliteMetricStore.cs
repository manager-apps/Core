using System.Text.Json;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Microsoft.Data.Sqlite;

namespace Agent.WindowsService.Infrastructure.Store;

/// <summary>
/// SQLite-based metric store with ACID transactions and efficient indexing
/// </summary>
public class SqliteMetricStore : IMetricStore, IDisposable
{
  private readonly string _connectionString;
  private readonly ILogger<SqliteMetricStore> _logger;
  private bool _disposed;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = false
  };

  public SqliteMetricStore(ILogger<SqliteMetricStore> logger)
  {
    _logger = logger;
    _connectionString = PathConfig.MetricsConnectionString;

    InitializeDatabase();
  }

  private void InitializeDatabase()
  {
    try
    {
      Directory.CreateDirectory(PathConfig.BaseDirectory);

      using var connection = new SqliteConnection(_connectionString);
      connection.Open();

      var command = connection.CreateCommand();
      command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Metrics (
          Id INTEGER PRIMARY KEY AUTOINCREMENT,
          Type INTEGER NOT NULL,
          Name TEXT NOT NULL,
          Value REAL NOT NULL,
          Unit TEXT NOT NULL,
          TimestampUtc TEXT NOT NULL,
          Metadata TEXT,
          CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
        );

        CREATE INDEX IF NOT EXISTS idx_metrics_timestamp ON Metrics(TimestampUtc);
        CREATE INDEX IF NOT EXISTS idx_metrics_type ON Metrics(Type);
      ";
      command.ExecuteNonQuery();

      _logger.LogInformation("SQLite database initialized");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize SQLite database");
      throw;
    }
  }

  public async Task StoreAsync(IReadOnlyList<Domain.Metric> metrics, CancellationToken cancellationToken)
  {
    if (metrics.Count == 0)
    {
      _logger.LogDebug("No metrics to store");
      return;
    }

    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      foreach (var metric in metrics)
      {
        var command = connection.CreateCommand();
        command.CommandText = @"
          INSERT INTO Metrics (Type, Name, Value, Unit, TimestampUtc, Metadata)
          VALUES (@type, @name, @value, @unit, @timestamp, @metadata)
        ";

        command.Parameters.AddWithValue("@type", (int)metric.Type);
        command.Parameters.AddWithValue("@name", metric.Name);
        command.Parameters.AddWithValue("@value", metric.Value);
        command.Parameters.AddWithValue("@unit", metric.Unit);
        command.Parameters.AddWithValue("@timestamp", metric.TimestampUtc.ToString("O"));
        command.Parameters.AddWithValue("@metadata",
          metric.Metadata != null
            ? JsonSerializer.Serialize(metric.Metadata, JsonOptions)
            : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
      }

      await transaction.CommitAsync(cancellationToken);
      _logger.LogInformation("Stored {Count} metrics to SQLite", metrics.Count);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cancellationToken);
      _logger.LogError(ex, "Failed to store metrics");
      throw;
    }
  }

  public async Task<IReadOnlyList<Domain.Metric>> GetAllAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "SELECT Type, Name, Value, Unit, TimestampUtc, Metadata FROM Metrics ORDER BY TimestampUtc";

    var metrics = new List<Domain.Metric>();

    try
    {
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        metrics.Add(new Domain.Metric
        {
          Type = (Domain.MetricType)reader.GetInt32(0),
          Name = reader.GetString(1),
          Value = reader.GetDouble(2),
          Unit = reader.GetString(3),
          TimestampUtc = DateTime.Parse(reader.GetString(4)),
          Metadata = reader.IsDBNull(5)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(5), JsonOptions)
        });
      }

      _logger.LogInformation("Retrieved {Count} metrics from SQLite", metrics.Count);
      return metrics;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve metrics");
      throw;
    }
  }

  public async Task RemoveAllAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Metrics";

    try
    {
      var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
      _logger.LogInformation("Deleted {Count} metrics from SQLite", deleted);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete metrics");
      throw;
    }
  }

  /// <summary>
  /// Cleanup old metrics older than specified age
  /// </summary>
  public async Task CleanupOldMetricsAsync(TimeSpan maxAge, CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Metrics WHERE TimestampUtc < @cutoff";
    command.Parameters.AddWithValue("@cutoff", DateTime.UtcNow.Subtract(maxAge).ToString("O"));

    try
    {
      var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
      if (deleted > 0)
      {
        _logger.LogInformation("Cleaned up {Count} old metrics", deleted);
      }
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to cleanup old metrics");
    }
  }

  public void Dispose()
  {
    if (_disposed) return;

    SqliteConnection.ClearAllPools();
    _disposed = true;

    GC.SuppressFinalize(this);
  }
}

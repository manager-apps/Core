using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Agent.WindowsService.Utils;
using Microsoft.Data.Sqlite;

namespace Agent.WindowsService.Infrastructure.Store;

/// <summary>
/// SQLite-based instruction store with ACID transactions
/// </summary>
public class SqliteInstructionStore : IInstructionStore, IDisposable
{
  private readonly ILogger<SqliteInstructionStore> _logger;
  private readonly string _connectionString;
  private bool _disposed;

  public SqliteInstructionStore(
    ILogger<SqliteInstructionStore> logger)
  {
    _connectionString = PathConfig.InstructionsConnectionString;
    _logger = logger;

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
        CREATE TABLE IF NOT EXISTS Instructions (
          AssociativeId INTEGER PRIMARY KEY,
          Type INTEGER NOT NULL,
          Payload TEXT NOT NULL,
          CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
        );

        CREATE TABLE IF NOT EXISTS InstructionResults (
          AssociativeId INTEGER PRIMARY KEY,
          Success INTEGER NOT NULL,
          Output TEXT,
          Error TEXT,
          CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
        );

        CREATE INDEX IF NOT EXISTS idx_instructions_associative ON Instructions(AssociativeId);
        CREATE INDEX IF NOT EXISTS idx_results_associative ON InstructionResults(AssociativeId);
      ";

      command.ExecuteNonQuery();
      _logger.LogInformation("SQLite instruction database initialized");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize SQLite instruction database");
      throw;
    }
  }

  public async Task SaveAsync(
    IEnumerable<Instruction> instructions,
    CancellationToken cancellationToken)
  {
    var instructionList = instructions.ToList();
    if (instructionList.Count == 0)
    {
      _logger.LogDebug("No instructions to save");
      return;
    }

    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
    try
    {
      foreach (var instruction in instructionList)
      {
        var command = connection.CreateCommand();
        command.CommandText = @"
          INSERT OR REPLACE INTO Instructions (AssociativeId, Type, Payload)
          VALUES (@associativeId, @type, @payload)
        ";

        command.Parameters.AddWithValue("@associativeId", instruction.AssociativeId);
        command.Parameters.AddWithValue("@type", (int)instruction.Type);
        command.Parameters.AddWithValue("@payload", InstructionUtils.SerializePayload(instruction.Payload));

        await command.ExecuteNonQueryAsync(cancellationToken);
      }

      await transaction.CommitAsync(cancellationToken);
      _logger.LogInformation("Saved {Count} instructions to SQLite", instructionList.Count);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cancellationToken);
      _logger.LogError(ex, "Failed to save instructions");
      throw;
    }
  }

  public async Task<IReadOnlyList<InstructionResult>> GetResultsAsync(
    CancellationToken cancellationToken,
    int limit = 50)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT AssociativeId, Success, Output, Error
      FROM InstructionResults
      ORDER BY CreatedAt
      LIMIT @limit
    ";
    command.Parameters.AddWithValue("@limit", limit);

    var results = new List<InstructionResult>();
    try
    {
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        results.Add(new InstructionResult
        {
          AssociativeId = reader.GetInt64(0),
          Success = reader.GetInt32(1) == 1,
          Output = reader.IsDBNull(2) ? null : reader.GetString(2),
          Error = reader.IsDBNull(3) ? null : reader.GetString(3)
        });
      }

      _logger.LogInformation("Retrieved {Count} instruction results from SQLite", results.Count);
      return results;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve instruction results");
      throw;
    }
  }

  public async Task<IReadOnlyList<Instruction>> GetAsync(
    CancellationToken cancellationToken,
    int limit = 50)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = @"
      SELECT AssociativeId, Type, Payload
      FROM Instructions
      ORDER BY CreatedAt
      LIMIT @limit
    ";
    command.Parameters.AddWithValue("@limit", limit);

    var instructions = new List<Instruction>();
    try
    {
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        var associativeId = reader.GetInt64(0);
        var type = (InstructionType)reader.GetInt32(1);
        var payloadJson = reader.GetString(2);

        var payload = InstructionUtils.DeserializePayload(type, payloadJson);

        instructions.Add(new Instruction
        {
          AssociativeId = associativeId,
          Type = type,
          Payload = payload
        });
      }

      _logger.LogInformation("Retrieved {Count} instructions from SQLite", instructions.Count);
      return instructions;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve instructions");
      throw;
    }
  }

  public async Task SaveResultsAsync(
    IEnumerable<InstructionResult> results,
    CancellationToken cancellationToken)
  {
    var resultList = results.ToList();
    if (resultList.Count == 0)
    {
      _logger.LogDebug("No instruction results to save");
      return;
    }

    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
    try
    {
      foreach (var result in resultList)
      {
        var command = connection.CreateCommand();

        command.CommandText = @"
          INSERT OR REPLACE INTO InstructionResults (AssociativeId, Success, Output, Error)
          VALUES (@associativeId, @success, @output, @error)
        ";

        command.Parameters.AddWithValue("@associativeId", result.AssociativeId);
        command.Parameters.AddWithValue("@success", result.Success ? 1 : 0);
        command.Parameters.AddWithValue("@output", result.Output ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@error", result.Error ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
      }

      await transaction.CommitAsync(cancellationToken);
      _logger.LogInformation("Saved {Count} instruction results to SQLite", resultList.Count);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cancellationToken);
      _logger.LogError(ex, "Failed to save instruction results");
      throw;
    }
  }

  public async Task RemoveAsync(
    IEnumerable<long> asociativeIds,
    CancellationToken cancellationToken)
  {
    var idList = asociativeIds.ToList();
    if (idList.Count == 0)
    {
      _logger.LogDebug("No instructions to delete");
      return;
    }

    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);
    var command = connection.CreateCommand();
    var parameters = string.Join(", ", idList.Select((_, index) => $"@id{index}"));
    command.CommandText = $"DELETE FROM Instructions WHERE AssociativeId IN ({parameters})";
    for (int i = 0; i < idList.Count; i++)
    {
      command.Parameters.AddWithValue($"@id{i}", idList[i]);
    }
    try
    {
      var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
      _logger.LogInformation("Deleted {Count} instructions from SQLite", deleted);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete instructions");
      throw;
    }
  }

  public async Task RemoveResultsAsync(
    IEnumerable<long> associativeIds,
    CancellationToken cancellationToken)
  {
    var idList = associativeIds.ToList();
    if (idList.Count == 0)
    {
      _logger.LogDebug("No instruction results to delete");
      return;
    }

    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    var parameters = string.Join(", ", idList.Select((_, index) => $"@id{index}"));
    command.CommandText = $"DELETE FROM InstructionResults WHERE AssociativeId IN ({parameters})";

    for (int i = 0; i < idList.Count; i++)
    {
      command.Parameters.AddWithValue($"@id{i}", idList[i]);
    }

    try
    {
      var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
      _logger.LogInformation("Deleted {Count} instruction results from SQLite", deleted);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete instruction results");
      throw;
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

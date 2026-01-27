using System.Text.Json;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;
using Common.Messages;
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

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = false
  };

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
          Id INTEGER PRIMARY KEY AUTOINCREMENT,
          AssociativeId INTEGER NOT NULL UNIQUE,
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

  public async Task SaveAsync(IEnumerable<Instruction> instructions, CancellationToken cancellationToken)
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
        command.Parameters.AddWithValue("@payload", Instruction.SerializePayload(instruction.Payload));

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

  public async Task<IReadOnlyList<Instruction>> GetAllAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "SELECT AssociativeId, Type, Payload FROM Instructions ORDER BY CreatedAt";

    var instructions = new List<Instruction>();
    try
    {
      await using var reader = await command.ExecuteReaderAsync(cancellationToken);
      while (await reader.ReadAsync(cancellationToken))
      {
        var associativeId = reader.GetInt64(0);
        var type = (InstructionType)reader.GetInt32(1);
        var payloadJson = reader.GetString(2);

        var payload = Instruction.DeserializePayload(type, payloadJson);

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

  public async Task SaveResultAsync(InstructionResult result, CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = @"
      INSERT OR REPLACE INTO InstructionResults (AssociativeId, Success, Output, Error)
      VALUES (@associativeId, @success, @output, @error)
    ";

    command.Parameters.AddWithValue("@associativeId", result.AssociativeId);
    command.Parameters.AddWithValue("@success", result.Success ? 1 : 0);
    command.Parameters.AddWithValue("@output", result.Output ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@error", result.Error ?? (object)DBNull.Value);

    try
    {
      await command.ExecuteNonQueryAsync(cancellationToken);
      _logger.LogDebug("Saved instruction result for {Id}", result.AssociativeId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to save instruction result");
      throw;
    }
  }

  public async Task SaveResultsAsync(IEnumerable<InstructionResult> results, CancellationToken cancellationToken)
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

  public async Task<IReadOnlyList<InstructionResult>> GetAllResultsAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "SELECT AssociativeId, Success, Output, Error FROM InstructionResults ORDER BY CreatedAt";

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

  public async Task RemoveAllResultsAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM InstructionResults";

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

  public async Task RemoveAllAsync(CancellationToken cancellationToken)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Instructions";

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

  public void Dispose()
  {
    if (_disposed) return;

    SqliteConnection.ClearAllPools();
    _disposed = true;

    GC.SuppressFinalize(this);
  }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Tests.Helpers;

internal static class DbContextFactory
{
  public static AppDbContext Create()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }

  /// <summary>
  /// Creates a SQLite in-memory context. Required for queries that use AsSplitQuery()
  /// (e.g. SyncHandler), which is not supported by the InMemory provider.
  /// The caller must dispose both the returned context and connection.
  /// </summary>
  public static AppDbContext CreateSqlite(out SqliteConnection connection)
  {
    connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();

    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(connection)
      .Options;

    var db = new AppDbContext(options);
    db.Database.EnsureCreated();
    return db;
  }
}

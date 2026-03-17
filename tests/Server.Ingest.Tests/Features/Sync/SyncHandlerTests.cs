using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Common.Messages;
using Server.Domain;
using Server.Ingest.Features.Sync;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Sync;

public class SyncHandlerTests
{
  private static ClaimsPrincipal CreatePrincipal(string? agentName)
  {
    var identity = agentName is null
      ? new ClaimsIdentity()
      : new ClaimsIdentity([new Claim(ClaimTypes.Name, agentName)], "Certificate");
    return new ClaimsPrincipal(identity);
  }

  private static SyncMessageRequest CreateRequest(
    string osVersion = "Windows 11",
    string machineName = "PC-001",
    int processorCount = 4,
    long memoryBytes = 8_000_000_000L)
  {
    var hardware = new HardwareMessage(osVersion, machineName, processorCount, memoryBytes);
    var config = new ConfigMessage(60, 120, 300, 5, 10, 50, 30, ["cpu"], ["shell"]);
    return new SyncMessageRequest(hardware, config);
  }

  [Fact]
  public async Task HandleAsync_ReturnsUnauthorized_WhenAgentNameIsEmpty()
  {
    using var connection = new SqliteConnection("Data Source=:memory:");
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal(null), CreateRequest(), "tag1", "1.0.0", CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest(), "tag1", "1.0.0", CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_CreatesHardware_WhenNoneExists()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest("Windows 11", "PC-001", 8, 16_000_000_000L),
      "tag1", "1.0.0", CancellationToken.None);

    var hardware = db.Hardwares.Single();
    Assert.Equal("Windows 11", hardware.OsVersion);
    Assert.Equal("PC-001", hardware.MachineName);
    Assert.Equal(8, hardware.ProcessorCount);
    Assert.Equal(16_000_000_000L, hardware.TotalMemoryBytes);
  }

  [Fact]
  public async Task HandleAsync_UpdatesHardware_WhenItAlreadyExists()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    // First sync: creates hardware
    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest("Windows 10", "PC-001", 4, 8_000_000_000L),
      "tag1", "1.0.0", CancellationToken.None);

    // Second sync: updates hardware
    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest("Windows 11", "PC-001", 16, 32_000_000_000L),
      "tag1", "2.0.0", CancellationToken.None);

    Assert.Equal(1, db.Hardwares.Count());
    var hardware = db.Hardwares.Single();
    Assert.Equal("Windows 11", hardware.OsVersion);
    Assert.Equal(16, hardware.ProcessorCount);
    Assert.Equal(32_000_000_000L, hardware.TotalMemoryBytes);
  }

  [Fact]
  public async Task HandleAsync_CreatesConfig_WhenNoneExists()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest(), "tag1", "1.0.0", CancellationToken.None);

    Assert.Equal(1, db.Configs.Count());
  }

  [Fact]
  public async Task HandleAsync_UpdatesConfig_WhenItAlreadyExists()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    // First sync: creates config
    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest(), "tag1", "1.0.0", CancellationToken.None);

    // Second sync: updates config
    // ConfigMessage(AuthenticationExitIntervalSeconds, RunningExitInterval, ExecutionExitInterval,
    //   InstructionsExecutionLimit, InstructionResultsSendLimit, MetricsSendLimit, IterationDelaySeconds, ...)
    var updatedConfig = new ConfigMessage(180, 360, 720, 20, 40, 200, 90, ["cpu", "memory"], ["shell"]);
    var updatedRequest = new SyncMessageRequest(
      new HardwareMessage("Windows 11", "PC-001", 4, 8_000_000_000L), updatedConfig);

    await handler.HandleAsync(
      CreatePrincipal("PC-001"), updatedRequest, "tag1", "2.0.0", CancellationToken.None);

    Assert.Equal(1, db.Configs.Count());
    var config = db.Configs.Single();
    Assert.Equal(90, config.IterationDelaySeconds);
    Assert.Equal(20, config.InstructionsExecutionLimit);
  }

  [Fact]
  public async Task HandleAsync_UpdatesLastSeen_WithVersionAndTag()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    db.Agents.Add(Agent.Create("PC-001", "old-tag"));
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);

    await handler.HandleAsync(
      CreatePrincipal("PC-001"), CreateRequest(), "new-tag", "2.5.0", CancellationToken.None);

    var agent = db.Agents.Single();
    Assert.Equal("new-tag", agent.CurrentTag);
    Assert.Equal("2.5.0", agent.Version);
  }

  [Fact]
  public async Task HandleAsync_ReturnsConfig_OnSuccess()
  {
    using var db = DbContextFactory.CreateSqlite(out var conn);
    using var _ = conn;

    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new SyncHandler(NullLogger<SyncHandler>.Instance, db);
    var request = CreateRequest();

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), request, "tag1", "1.0.0", CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(request.Config.IterationDelaySeconds, result.Value.Config.IterationDelaySeconds);
    Assert.Equal(request.Config.InstructionsExecutionLimit, result.Value.Config.InstructionsExecutionLimit);
    Assert.Equal(request.Config.AllowedCollectors, result.Value.Config.AllowedCollectors);
  }
}

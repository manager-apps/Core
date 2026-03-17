using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Hardware.Get;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentHardwareGetHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentHardwareGetHandler(NullLogger<AgentHardwareGetHandler>.Instance, db);

    var result = await handler.HandleAsync(999, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentHasNoHardware()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new AgentHardwareGetHandler(NullLogger<AgentHardwareGetHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsHardware_WhenAgentHasHardware()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var hardware = Hardware.Create("Windows 11", "WORKSTATION-01", 8, 16_000_000_000L);
    agent.AssignHardware(hardware);
    db.Hardwares.Add(hardware);
    await db.SaveChangesAsync();

    var handler = new AgentHardwareGetHandler(NullLogger<AgentHardwareGetHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("Windows 11", result.Value.OsVersion);
    Assert.Equal("WORKSTATION-01", result.Value.MachineName);
    Assert.Equal(8, result.Value.ProcessorCount);
    Assert.Equal(16_000_000_000L, result.Value.TotalMemoryBytes);
  }
}

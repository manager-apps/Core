using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.GetById;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentGetByIdHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentGetByIdHandler(NullLogger<AgentGetByIdHandler>.Instance, db);

    var result = await handler.HandleAsync(999, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsAgent_WhenFound()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new AgentGetByIdHandler(NullLogger<AgentGetByIdHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("TestAgent", result.Value.Name);
    Assert.Equal("TagA", result.Value.SourceTag);
  }

  [Fact]
  public async Task HandleAsync_ReturnsNullConfigAndHardware_WhenNotAssigned()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new AgentGetByIdHandler(NullLogger<AgentGetByIdHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Null(result.Value.Config);
    Assert.Null(result.Value.Hardware);
  }
}

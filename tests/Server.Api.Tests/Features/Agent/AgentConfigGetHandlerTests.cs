using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Config.Get;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentConfigGetHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentConfigGetHandler(NullLogger<AgentConfigGetHandler>.Instance, db);

    var result = await handler.HandleAsync(999, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentHasNoConfig()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var handler = new AgentConfigGetHandler(NullLogger<AgentConfigGetHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsConfig_WhenAgentHasConfig()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var config = Config.Create(30, 300, 300, 300, 5, 5, 100, [], []);
    agent.AssignConfig(config);
    db.Configs.Add(config);
    await db.SaveChangesAsync();

    var handler = new AgentConfigGetHandler(NullLogger<AgentConfigGetHandler>.Instance, db);

    var result = await handler.HandleAsync(agent.Id, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(30, result.Value.IterationDelaySeconds);
    Assert.Equal(5, result.Value.InstructionsExecutionLimit);
  }
}

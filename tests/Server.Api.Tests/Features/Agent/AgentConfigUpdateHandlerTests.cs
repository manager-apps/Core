using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Config.Update;
using Xunit;
using Server.Api.Features.Config;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentConfigUpdateHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentConfigUpdateHandler(NullLogger<AgentConfigUpdateHandler>.Instance, db);
    var request = new ConfigUpdateRequest(MetricsSendLimit: 50);

    var result = await handler.HandleAsync(999, request, CancellationToken.None);

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

    var handler = new AgentConfigUpdateHandler(NullLogger<AgentConfigUpdateHandler>.Instance, db);
    var request = new ConfigUpdateRequest(MetricsSendLimit: 50);

    var result = await handler.HandleAsync(agent.Id, request, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_UpdatesConfig_AndCreatesConfigInstruction()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("TestAgent", "TagA");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var config = Config.Create(30, 300, 300, 300, 5, 5, 100, [], []);
    agent.AssignConfig(config);
    db.Configs.Add(config);
    await db.SaveChangesAsync();

    var handler = new AgentConfigUpdateHandler(NullLogger<AgentConfigUpdateHandler>.Instance, db);
    var request = new ConfigUpdateRequest(MetricsSendLimit: 200, InstructionsExecutionLimit: 10);

    var result = await handler.HandleAsync(agent.Id, request, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(200, result.Value.MetricsSendLimit);
    Assert.Equal(10, result.Value.InstructionsExecutionLimit);

    var instruction = db.Instructions.Single();
    Assert.Equal(agent.Id, instruction.AgentId);
    Assert.Equal(InstructionType.Config, instruction.Type);
    Assert.Equal(InstructionState.Pending, instruction.State);
  }
}

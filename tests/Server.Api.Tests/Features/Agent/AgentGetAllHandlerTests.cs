using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.GetAll;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentGetAllHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsEmpty_WhenNoAgents()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentGetAllHandler(NullLogger<AgentGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(CancellationToken.None);

    Assert.Empty(result);
  }

  [Fact]
  public async Task HandleAsync_ReturnsAllAgents()
  {
    using var db = DbContextFactory.Create();
    db.Agents.AddRange(
      Agent.Create("Agent1", "TagA"),
      Agent.Create("Agent2", "TagB"));
    await db.SaveChangesAsync();

    var handler = new AgentGetAllHandler(NullLogger<AgentGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(CancellationToken.None);

    Assert.Equal(2, result.Count());
  }

  [Fact]
  public async Task HandleAsync_MapsAgentFieldsCorrectly()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("TestAgent", "TagX"));
    await db.SaveChangesAsync();

    var handler = new AgentGetAllHandler(NullLogger<AgentGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(CancellationToken.None);
    var agent = result.Single();

    Assert.Equal("TestAgent", agent.Name);
    Assert.Equal("TagX", agent.SourceTag);
    Assert.Equal("TagX", agent.CurrentTag);
  }
}

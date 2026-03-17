using System.Security.Claims;
using System.Text.Json;
using Common.Messages;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Domain;
using Server.Ingest.Features.Report.Create;
using Server.Ingest.Tests.Helpers;
using Xunit;

namespace Server.Ingest.Tests.Features.Report;

public class ReportCreateHandlerTests
{
  private static ClaimsPrincipal CreatePrincipal(string? agentName)
  {
    var identity = agentName is null
      ? new ClaimsIdentity()
      : new ClaimsIdentity([new Claim(ClaimTypes.Name, agentName)], "Certificate");
    return new ClaimsPrincipal(identity);
  }

  private static MetricMessage CreateMetric()
    => new("cpu", "cpu_usage", 42.5, "%", DateTime.UtcNow, null);

  private static InstructionResultMessage CreateInstructionResult(long id)
    => new(id, true, "output", null);

  [Fact]
  public async Task HandleAsync_ReturnsUnauthorized_WhenAgentNameIsEmpty()
  {
    using var db = DbContextFactory.Create();
    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var request = new ReportMessageRequest([], []);
    var result = await handler.HandleAsync(CreatePrincipal(null), request, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Unauthorized", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsAgentNotFound_WhenAgentDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var request = new ReportMessageRequest([], []);
    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), request, CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_CreatesOutboxMessage_ForEachMetric()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);
    var request = new ReportMessageRequest([CreateMetric(), CreateMetric()], []);

    await handler.HandleAsync(CreatePrincipal("PC-001"), request, CancellationToken.None);

    Assert.Equal(2, db.OutboxMessages.Count(m => m.Type == "AgentMetricsEvent"));
  }

  [Fact]
  public async Task HandleAsync_CreatesOutboxMessage_ForEachInstructionResult()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);
    var request = new ReportMessageRequest([], [CreateInstructionResult(1), CreateInstructionResult(2)]);

    await handler.HandleAsync(CreatePrincipal("PC-001"), request, CancellationToken.None);

    Assert.Equal(2, db.OutboxMessages.Count(m => m.Type == "AgentInstructionResultEvent"));
  }

  [Fact]
  public async Task HandleAsync_ReturnsEmptyInstructions_WhenNoPendingInstructions()
  {
    using var db = DbContextFactory.Create();
    db.Agents.Add(Agent.Create("PC-001", "tag1"));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), new ReportMessageRequest([], []), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.Instructions);
  }

  [Fact]
  public async Task HandleAsync_ReturnsPendingInstructions_AndMarksThemAsDispatched()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var payloadJson = JsonSerializer.Serialize(new ShellCommandPayload("echo hello", 5000));
    db.Instructions.Add(Instruction.Create(agent.Id, InstructionType.Shell, payloadJson));
    db.Instructions.Add(Instruction.Create(agent.Id, InstructionType.Shell, payloadJson));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), new ReportMessageRequest([], []), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Instructions.Count());
    Assert.All(db.Instructions.ToList(), i => Assert.Equal(InstructionState.Dispatched, i.State));
  }

  [Fact]
  public async Task HandleAsync_ReturnsAtMostTenInstructions()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    db.Agents.Add(agent);
    await db.SaveChangesAsync();

    var payloadJson = JsonSerializer.Serialize(new ShellCommandPayload("echo hi", 5000));
    for (var i = 0; i < 15; i++)
      db.Instructions.Add(Instruction.Create(agent.Id, InstructionType.Shell, payloadJson));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), new ReportMessageRequest([], []), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(10, result.Value.Instructions.Count());
  }

  [Fact]
  public async Task HandleAsync_DoesNotReturnInstructions_ForOtherAgents()
  {
    using var db = DbContextFactory.Create();
    var agent1 = Agent.Create("PC-001", "tag1");
    var agent2 = Agent.Create("PC-002", "tag1");
    db.Agents.AddRange(agent1, agent2);
    await db.SaveChangesAsync();

    var payloadJson = JsonSerializer.Serialize(new ShellCommandPayload("echo hi", 5000));
    db.Instructions.Add(Instruction.Create(agent2.Id, InstructionType.Shell, payloadJson));
    await db.SaveChangesAsync();

    var handler = new ReportCreateHandler(NullLogger<ReportCreateHandler>.Instance, db);

    var result = await handler.HandleAsync(
      CreatePrincipal("PC-001"), new ReportMessageRequest([], []), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.Instructions);
  }
}

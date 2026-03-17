using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Instruction.GetAll;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentInstructionsGetAllHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsEmpty_WhenNoInstructionsForAgent()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentInstructionsGetAllHandler(NullLogger<AgentInstructionsGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(1, CancellationToken.None);

    Assert.Empty(result);
  }

  [Fact]
  public async Task HandleAsync_ReturnsOnlyInstructionsForSpecifiedAgent()
  {
    using var db = DbContextFactory.Create();
    db.Instructions.AddRange(
      Instruction.Create(1, InstructionType.Shell, "{\"command\":\"echo hello\",\"timeout\":5000}"),
      Instruction.Create(1, InstructionType.Gpo, "{\"name\":\"policy\",\"value\":\"val\"}"),
      Instruction.Create(2, InstructionType.Shell, "{\"command\":\"echo world\",\"timeout\":5000}"));
    await db.SaveChangesAsync();

    var handler = new AgentInstructionsGetAllHandler(NullLogger<AgentInstructionsGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(1, CancellationToken.None);

    Assert.Equal(2, result.Count());
    Assert.All(result, r => Assert.Equal(1, r.AgentId));
  }

  [Fact]
  public async Task HandleAsync_MapsInstructionFieldsCorrectly()
  {
    using var db = DbContextFactory.Create();
    db.Instructions.Add(Instruction.Create(1, InstructionType.Shell, "{\"command\":\"ls\",\"timeout\":3000}"));
    await db.SaveChangesAsync();

    var handler = new AgentInstructionsGetAllHandler(NullLogger<AgentInstructionsGetAllHandler>.Instance, db);

    var result = await handler.HandleAsync(1, CancellationToken.None);
    var instruction = result.Single();

    Assert.Equal(InstructionType.Shell, instruction.Type);
    Assert.Equal(InstructionState.Pending, instruction.State);
    Assert.Null(instruction.Output);
    Assert.Null(instruction.Error);
  }
}

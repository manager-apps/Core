using Server.Api.Features.Instruction.GetById;
using Xunit;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.InstructionHandlers;

public class InstructionGetByIdHandlerTests
{
  [Fact]
  public async Task HandleAsync_ReturnsNotFound_WhenInstructionDoesNotExist()
  {
    using var db = DbContextFactory.Create();
    var handler = new InstructionGetByIdHandler(db);

    var result = await handler.HandleAsync(999, CancellationToken.None);

    Assert.True(result.IsFailure);
    Assert.Equal("NotFound", result.Error.Code);
  }

  [Fact]
  public async Task HandleAsync_ReturnsInstruction_WhenFound()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1, InstructionType.Shell, "{\"command\":\"echo hi\",\"timeout\":5000}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var handler = new InstructionGetByIdHandler(db);

    var result = await handler.HandleAsync(instruction.Id, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(instruction.Id, result.Value.Id);
    Assert.Equal(1, result.Value.AgentId);
    Assert.Equal(InstructionType.Shell, result.Value.Type);
    Assert.Equal(InstructionState.Pending, result.Value.State);
  }
}

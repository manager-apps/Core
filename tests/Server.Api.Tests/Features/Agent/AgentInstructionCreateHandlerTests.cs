using Microsoft.Extensions.Logging.Abstractions;
using Server.Api.Features.Agent.Instruction.Create;
using Xunit;
using Server.Api.Features.Instruction;
using Server.Api.Tests.Helpers;
using Server.Domain;

namespace Server.Api.Tests.AgentHandlers;

public class AgentInstructionCreateHandlerTests
{
  [Fact]
  public async Task HandleShellCommandAsync_CreatesShellInstruction()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentInstructionCreateHandler(NullLogger<AgentInstructionCreateHandler>.Instance, db);
    var request = new CreateShellCommandRequest("echo hello", 5000);

    var result = await handler.HandleShellCommandAsync(1, request, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(1, result.Value.AgentId);
    Assert.Equal(InstructionType.Shell, result.Value.Type);
    Assert.Equal(InstructionState.Pending, result.Value.State);
  }

  [Fact]
  public async Task HandleShellCommandAsync_PersistsInstructionToDatabase()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentInstructionCreateHandler(NullLogger<AgentInstructionCreateHandler>.Instance, db);
    var request = new CreateShellCommandRequest("ipconfig /all", 10000);

    await handler.HandleShellCommandAsync(42, request, CancellationToken.None);

    var saved = db.Instructions.Single();
    Assert.Equal(42, saved.AgentId);
    Assert.Equal(InstructionType.Shell, saved.Type);
    Assert.Contains("ipconfig", saved.PayloadJson);
  }

  [Fact]
  public async Task HandleGpoSetAsync_CreatesGpoInstruction()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentInstructionCreateHandler(NullLogger<AgentInstructionCreateHandler>.Instance, db);
    var request = new CreateGpoSetRequest("DisableUSB", "Enabled");

    var result = await handler.HandleGpoSetAsync(1, request, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(InstructionType.Gpo, result.Value.Type);
    Assert.Equal(InstructionState.Pending, result.Value.State);
  }

  [Fact]
  public async Task HandleGpoSetAsync_PersistsInstructionToDatabase()
  {
    using var db = DbContextFactory.Create();
    var handler = new AgentInstructionCreateHandler(NullLogger<AgentInstructionCreateHandler>.Instance, db);
    var request = new CreateGpoSetRequest("DisableUSB", "Enabled");

    await handler.HandleGpoSetAsync(10, request, CancellationToken.None);

    var saved = db.Instructions.Single();
    Assert.Equal(10, saved.AgentId);
    Assert.Equal(InstructionType.Gpo, saved.Type);
    Assert.Contains("DisableUSB", saved.PayloadJson);
  }
}

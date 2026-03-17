using System.Text.Json;
using Common;
using Common.Events;
using Common.Messages;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Domain;
using Server.InstructionWorker.Tests.Helpers;
using Xunit;

namespace Server.InstructionWorker.Tests;

public class InstructionResultProcessorTests
{
  private static InstructionResultProcessor CreateProcessor()
    => new(NullLogger<InstructionResultProcessor>.Instance);

  private static OutboxMessage CreateMessage(string payloadJson)
    => OutboxMessage.Create(payloadJson, nameof(AgentInstructionResultEvent));

  private static string SerializeEvent(long instructionId, bool success, string? output = null, string? error = null)
  {
    var evt = new AgentInstructionResultEvent(
      new InstructionResultMessage(instructionId, success, output, error));
    return JsonSerializer.Serialize(evt, JsonOptions.Default);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsFalse_WhenPayloadJsonIsInvalid()
  {
    using var db = DbContextFactory.Create();
    var message = CreateMessage("not-valid-json{{{");

    var result = await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageFailed_WhenPayloadJsonIsInvalid()
  {
    using var db = DbContextFactory.Create();
    var message = CreateMessage("not-valid-json{{{");

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Failed, message.State);
    Assert.Equal(1, message.RetryCount);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsFalse_WhenInstructionNotFound()
  {
    using var db = DbContextFactory.Create();
    var message = CreateMessage(SerializeEvent(instructionId: 999, success: true, output: "out"));

    var result = await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageFailed_WhenInstructionNotFound()
  {
    using var db = DbContextFactory.Create();
    var message = CreateMessage(SerializeEvent(instructionId: 999, success: true));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Failed, message.State);
  }

  [Fact]
  public async Task ProcessAsync_MarksInstructionCompleted_WhenResultIsSuccess()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: true, output: "hello"));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(InstructionState.Completed, instruction.State);
    Assert.Equal("hello", instruction.Output);
  }

  [Fact]
  public async Task ProcessAsync_MarksInstructionFailed_WhenResultIsFailure()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: false, error: "timeout"));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(InstructionState.Failed, instruction.State);
    Assert.Equal("timeout", instruction.Error);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageProcessed_OnSuccess()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: true, output: "done"));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Processed, message.State);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsTrue_OnSuccess()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: true, output: "ok"));

    var result = await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  public async Task ProcessAsync_UsesUnknownError_WhenFailureHasNoErrorMessage()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: false, error: null));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal("Unknown error", instruction.Error);
  }

  [Fact]
  public async Task ProcessAsync_UsesEmptyOutput_WhenSuccessHasNoOutput()
  {
    using var db = DbContextFactory.Create();
    var instruction = Instruction.Create(1L, InstructionType.Shell, "{}");
    db.Instructions.Add(instruction);
    await db.SaveChangesAsync();

    var message = CreateMessage(SerializeEvent(instruction.Id, success: true, output: null));

    await CreateProcessor().ProcessAsync(message, db, CancellationToken.None);

    Assert.Equal(string.Empty, instruction.Output);
  }
}

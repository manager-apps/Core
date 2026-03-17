using System.Text.Json;
using Common;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.InstructionWorker.Infrastructure;

namespace Server.InstructionWorker;

public interface IInstructionResultProcessor
{
  Task<bool> ProcessAsync(OutboxMessage message, AppDbContext dbContext, CancellationToken ct);
}

internal sealed class InstructionResultProcessor(
  ILogger<InstructionResultProcessor> logger) : IInstructionResultProcessor
{
  public async Task<bool> ProcessAsync(OutboxMessage message, AppDbContext dbContext, CancellationToken ct)
  {
    try
    {
      var @event = JsonSerializer.Deserialize<AgentInstructionResultEvent>(
        message.PayloadJson, JsonOptions.Default);

      if (@event is null)
      {
        message.MarkAsFailed("Failed to deserialize AgentInstructionResultEvent");
        return false;
      }

      var instruction = await dbContext.Instructions
        .FirstOrDefaultAsync(i => i.Id == @event.InstructionResult.AssociatedId, ct);

      if (instruction is null)
      {
        logger.LogWarning("Instruction {InstructionId} not found", @event.InstructionResult.AssociatedId);
        message.MarkAsFailed($"Instruction {@event.InstructionResult.AssociatedId} not found");
        return false;
      }

      if (@event.InstructionResult.Success)
        instruction.MarkAsCompleted(@event.InstructionResult.Output ?? string.Empty);
      else
        instruction.MarkAsFailed(@event.InstructionResult.Error ?? "Unknown error");

      message.MarkAsProcessed();

      logger.LogDebug("Processed instruction {InstructionId} result: {Success}",
        instruction.Id, @event.InstructionResult.Success);

      return true;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      logger.LogWarning(ex, "Failed to process instruction message");
      message.MarkAsFailed(ex.Message);
      return false;
    }
  }
}

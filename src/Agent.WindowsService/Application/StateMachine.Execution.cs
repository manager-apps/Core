using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleExecutionEntryAsync()
  {
    _logger.LogInformation("Entering Execution state");
    try
    {
      var instructions = await _instrStore.GetAsync(Token);
      if (instructions.Count == 0)
      {
        _logger.LogInformation("No instructions to execute");
        await _machine.FireAsync(Triggers.ExecutionSuccess);
        return;
      }

      _logger.LogInformation("Processing {Count} instructions", instructions.Count);
      var results = new List<InstructionResult>();
      foreach (var instruction in instructions)
      {
        Token.ThrowIfCancellationRequested();

        try
        {
          var executor = _executors.FirstOrDefault(e => e.CanExecute(instruction.Type));
          if (executor == null)
          {
            _logger.LogWarning(
              "No executor found for instruction type {Type} (Id: {Id})",
              instruction.Type, instruction.AssociativeId);

            results.Add(new InstructionResult
            {
              AssociativeId = instruction.AssociativeId,
              Success = false,
              Output = null,
              Error = $"No executor found for instruction type: {instruction.Type}"
            });
            continue;
          }

          var result = await executor.ExecuteAsync(instruction, Token);
          results.Add(result);

          _logger.LogInformation(
            "Instruction {Id} executed with result: {Success}",
            instruction.AssociativeId, result.Success);
        }
        catch (OperationCanceledException)
        {
          throw;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex,
            "Failed to execute instruction {Id} of type {Type}",
            instruction.AssociativeId, instruction.Type);

          results.Add(new InstructionResult
          {
            AssociativeId = instruction.AssociativeId,
            Success = false,
            Output = null,
            Error = $"Execution failed: {ex.Message}"
          });
        }
      }

      await _instrStore.SaveResultsAsync(results, Token);
      await _instrStore.RemoveAsync(instructions.Select(x => x.AssociativeId), Token);

      _logger.LogInformation(
        "Execution completed: {Success}/{Total} instructions succeeded",
        results.Count(r => r.Success), results.Count);

      await _machine.FireAsync(Triggers.ExecutionSuccess);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Execution state cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Execution state failed");
      await _machine.FireAsync(Triggers.ExecutionFailure);
    }
  }

  private async Task HandleExecutionExitAsync()
  {
    _logger.LogInformation("Exiting Execution state");

    try
    {
      await Task.Delay(5000, Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Execution state exit delay cancelled");
    }
  }
}

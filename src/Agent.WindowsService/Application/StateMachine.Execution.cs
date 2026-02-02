using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleExecutionEntryAsync()
  {
    _logger.LogInformation("Entering Execution state");
    try
    {
      var instructions = await _instrStore.GetAllAsync(CancellationToken.None);
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

          var result = await executor.ExecuteAsync(instruction, CancellationToken.None);
          results.Add(result);

          _logger.LogInformation(
            "Instruction {Id} executed with result: {Success}",
            instruction.AssociativeId, result.Success);
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

      // Make it with pagination
      await _instrStore.SaveResultsAsync(results, CancellationToken.None);
      await _instrStore.RemoveAllAsync(CancellationToken.None);

      var successCount = results.Count(r => r.Success);
      _logger.LogInformation(
        "Execution completed: {Success}/{Total} instructions succeeded",
        successCount, results.Count);

      await _machine.FireAsync(Triggers.ExecutionSuccess);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in Execution");
      await _machine.FireAsync(Triggers.ExecutionFailure);
    }
  }

  private async Task HandleExecutionExitAsync()
  {
    _logger.LogInformation("Exiting Execution state");

    // Delaying, will be configurable later
    await Task.Delay(5000);
  }
}

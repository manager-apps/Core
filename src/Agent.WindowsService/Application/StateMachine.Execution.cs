using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleExecutionEntryAsync()
  {
    _logger.LogInformation("Entering Execution state");
    try
    {
      var config = await _configStore.GetAsync(Token);
      var instructions = await _instrStore.GetAsync(Token, config.InstructionsExecutionLimit);
      if (instructions.Count == 0)
      {
        _logger.LogInformation("No instructions to execute");
        await _machine.FireAsync(Triggers.ExecutionSuccess);
        return;
      }

      _logger.LogInformation("Processing {Count} instructions", instructions.Count);

      var results = await ExecuteInstructionsAsync(instructions, config);

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
      var config = await _configStore.GetAsync(Token);
      await Task.Delay(TimeSpan.FromSeconds(config.ExecutionExitIntervalSeconds), Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Execution state exit delay cancelled");
    }
  }

  private async Task<List<InstructionResult>> ExecuteInstructionsAsync(
    IReadOnlyList<Instruction> instructions,
    Configuration config)
  {
    var results = new List<InstructionResult>();
    foreach (var instruction in instructions)
    {
      Token.ThrowIfCancellationRequested();
      var result = await ExecuteSingleInstructionAsync(instruction, config);
      results.Add(result);
    }

    return results;
  }

  private async Task<InstructionResult> ExecuteSingleInstructionAsync(
    Instruction instruction,
    Configuration config)
  {
    try
    {
      if (!IsInstructionAllowed(instruction, config))
      {
        return CreateNotAllowedResult(instruction);
      }

      var executor = FindExecutor(instruction);
      if (executor is null)
      {
        return CreateNoExecutorResult(instruction);
      }

      var result = await executor.ExecuteAsync(instruction, Token);
      _logger.LogInformation(
        "Instruction {Id} executed with result: {Success}",
        instruction.AssociativeId, result.Success);

      return result;
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

      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Output = null,
        Error = $"Execution failed: {ex.Message}"
      };
    }
  }

  private bool IsInstructionAllowed(Instruction instruction, Configuration config)
  {
    if (instruction.Type == InstructionType.Config)
      return true;

    var typeName = instruction.Type.ToString();
    return config.AllowedInstructions.Count == 0 ||
           config.AllowedInstructions.Contains(typeName, StringComparer.OrdinalIgnoreCase);
  }

  private IInstructionExecutor? FindExecutor(Instruction instruction)
    => _executors.FirstOrDefault(e => e.CanExecute(instruction.Type));

  private InstructionResult CreateNotAllowedResult(Instruction instruction)
  {
    var typeName = instruction.Type.ToString();

    _logger.LogWarning(
      "Instruction type {Type} is not allowed by configuration (Id: {Id})",
      instruction.Type, instruction.AssociativeId);

    return new InstructionResult
    {
      AssociativeId = instruction.AssociativeId,
      Success = false,
      Output = null,
      Error = $"Instruction type '{typeName}' is not allowed by configuration"
    };
  }

  private InstructionResult CreateNoExecutorResult(Instruction instruction)
  {
    _logger.LogWarning(
      "No executor found for instruction type {Type} (Id: {Id})",
      instruction.Type, instruction.AssociativeId);

    return new InstructionResult
    {
      AssociativeId = instruction.AssociativeId,
      Success = false,
      Output = null,
      Error = $"No executor found for instruction type: {instruction.Type}"
    };
  }
}

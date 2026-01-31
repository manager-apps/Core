using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Abstraction;

public interface IInstructionExecutor
{
  /// <summary>
  /// Check if can execute the instruction type
  /// </summary>
  bool CanExecute(InstructionType type);

  /// <summary>
  /// Run the instruction
  /// </summary>
  Task<InstructionResult> ExecuteAsync(
    Instruction instruction, CancellationToken cancellationToken = default);
}

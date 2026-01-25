using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Abstraction;

public interface IInstructionStore
{
  /// <summary>
  /// Save instruction
  /// </summary>
  Task SaveAsync(IEnumerable<Instruction> instruction, CancellationToken cancellationToken);

  /// <summary>
  /// Get all instructions
  /// </summary>
  Task<IReadOnlyList<Instruction>> GetAllAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Save instruction result
  /// </summary>
  Task SaveResultAsync(InstructionResult result, CancellationToken cancellationToken);

  /// <summary>
  /// Save multiple instruction results
  /// </summary>
  Task SaveResultsAsync(IEnumerable<InstructionResult> results, CancellationToken cancellationToken);

  /// <summary>
  /// Get all instruction results
  /// </summary>
  Task<IReadOnlyList<InstructionResult>> GetAllResultsAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Remove all instruction results
  /// </summary>
  Task RemoveAllResultsAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Remove all instructions
  /// </summary>
  Task RemoveAllAsync(CancellationToken cancellationToken);
}

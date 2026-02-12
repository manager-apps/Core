using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Abstraction;

public interface IInstructionStore
{
  /// <summary>
  /// Save instruction
  /// </summary>
  Task SaveAsync(
    IEnumerable<Instruction> instruction,
    CancellationToken cancellationToken);

  /// <summary>
  /// Get all instructions
  /// </summary>
  Task<IReadOnlyList<Instruction>> GetAsync(
    CancellationToken cancellationToken,
    int limit = 50);

  /// <summary>
  /// Remove all instructions
  /// </summary>
  Task RemoveAsync(
    IEnumerable<long> associativeIds,
    CancellationToken cancellationToken);

  #region Results
  /// <summary>
  /// Save multiple instruction results
  /// </summary>
  Task SaveResultsAsync(
    IEnumerable<InstructionResult> results,
    CancellationToken cancellationToken);

  /// <summary>
  /// Get all instruction results
  /// </summary>
  Task<IReadOnlyList<InstructionResult>> GetResultsAsync(
    CancellationToken cancellationToken,
    int limit = 50);

  /// <summary>
  /// Remove all instruction results
  /// </summary>
  Task RemoveResultsAsync(
    IEnumerable<long> associativeIds,
    CancellationToken cancellationToken);

  #endregion
}

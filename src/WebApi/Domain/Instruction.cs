using System.ComponentModel.DataAnnotations;

namespace WebApi.Domain;

public enum InstructionType
{
  GpoSet = 1,
  ShellCommand,
  ConfigUpdate,
}

public enum InstructionState
{
  Pending = 1,
  Completed,
  Failed,
}

public class Instruction
{
  public long Id { get; init; }

  [Required]
  public long AgentId { get; private init; }

  [Required]
  public InstructionType Type { get; private init; }

  [Required]
  [MaxLength(4000)]
  public string PayloadJson { get; private init; } = null!;

  [Required]
  public InstructionState State { get; private set; }

  [MaxLength(4000)]
  public string? Output { get; private set; }

  [MaxLength(4000)]
  public string? Error { get; private set; }
  public DateTime CreatedAt { get; private init; }
  public DateTime? UpdatedAt { get; private set; }

  #region Navigation properties

  /// <summary>
  /// The agent associated with this instruction.
  /// </summary>
  public virtual Agent Agent { get; set; } = null!;

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new Instruction instance.
  /// </summary>
  public static Instruction Create(
    long agentId,
    InstructionType type,
    string payloadJson)
  {
    return new Instruction
    {
      AgentId = agentId,
      Type = type,
      PayloadJson = payloadJson,
      State = InstructionState.Pending,
      CreatedAt = DateTime.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Marks the instruction as completed with the given output.
  /// </summary>
  public void MarkAsCompleted(string output)
  {
    State = InstructionState.Completed;
    Output = output;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Marks the instruction as failed with the given error message.
  /// </summary>
  public void MarkAsFailed(string error)
  {
    State = InstructionState.Failed;
    Error = error;
    UpdatedAt = DateTime.UtcNow;
  }

  #endregion
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

public enum InstructionType
{
  GpoSet = 1,

  ShellCommand,

  ConfigUpdate,
}

public enum InstructionState
{
  Pending = 1,
  Dispatched,
  Completed,
  Failed,
}

[Index(nameof(AgentId)), Index(nameof(State))]
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
  public DateTimeOffset CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }

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
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Marks the instruction as dispatched to the agent.
  /// </summary>
  public void MarkAsDispatched()
  {
    State = InstructionState.Dispatched;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Marks the instruction as completed with the given output.
  /// </summary>
  public void MarkAsCompleted(string output)
  {
    State = InstructionState.Completed;
    Output = output;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Marks the instruction as failed with the given error message.
  /// </summary>
  public void MarkAsFailed(string error)
  {
    State = InstructionState.Failed;
    Error = error;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  #endregion
}

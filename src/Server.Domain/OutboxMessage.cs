using System.ComponentModel.DataAnnotations;

namespace Server.Domain;

public enum OutboxMessageState
{
  /// <summary>
  /// Message to be processed in the same process as the main application.
  /// </summary>
  InProcess = 1,

  /// <summary>
  /// Message to be processed by an external system.
  /// </summary>
  Processed,

  /// <summary>
  /// Message processing failed.
  /// </summary>
  Failed
}

public class OutboxMessage
{
  public Guid Id { get; init; }

  [Required]
  public OutboxMessageState State { get; private set; }

  [Required]
  [MaxLength(200)]
  public string Type { get; private init; } = null!;

  [Required]
  [MaxLength(8000)]
  public string PayloadJson { get; private init; } = null!;

  [Required]
  public int RetryCount { get; private set; }

  [MaxLength(2000)]
  public string? Error { get; private set; }
  public DateTime OccurredAt { get; private init; }
  public DateTime? UpdatedAt { get; private set; }

  #region Factory Methods

  /// <summary>
  /// Creates a new OutboxMessage in InProcess state.
  /// </summary>
  public static OutboxMessage Create(
    string payloadJson,
    string type)
  {
    return new OutboxMessage
    {
      Type = type,
      PayloadJson = payloadJson,
      State = OutboxMessageState.InProcess,
      OccurredAt = DateTime.UtcNow
    };
  }

  #endregion

  #region Domain Methods

  public void MarkAsProcessed()
  {
    State = OutboxMessageState.Processed;
    UpdatedAt = DateTime.UtcNow;
  }

  public void MarkAsFailed(string errorMessage)
  {
    State = OutboxMessageState.Failed;
    Error = errorMessage;
    RetryCount++;
    UpdatedAt = DateTime.UtcNow;
  }

  #endregion
}

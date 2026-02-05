using System.ComponentModel.DataAnnotations;

namespace Server.Domain;

/// <summary>
/// Server-side configuration overrides for an agent
/// </summary>
public class Config
{
  public long Id { get; init; }

  [Required]
  public long AgentId { get; init; }

  [Required]
  public int AuthenticationExitIntervalSeconds { get; private set; }

  [Required]
  public int SynchronizationExitIntervalSeconds { get; private set; }

  [Required]
  public int RunningExitIntervalSeconds { get; private set; }

  [Required]
  public int ExecutionExitIntervalSeconds { get; private set; }

  [Required]
  public int InstructionsExecutionLimit { get; private set; }

  [Required]
  public int InstructionResultsSendLimit { get; private set; }

  [Required]
  public int MetricsSendLimit { get; private set; }

  [MaxLength(1000)]
  public string? AllowedCollectors { get; private set; }

  [MaxLength(1000)]
  public string? AllowedInstructions { get; private set; }

  public DateTimeOffset? CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }

  #region Navigation properties

  public virtual Agent Agent { get; init; } = null!;

  #endregion

  #region Factory methods

  public static Config Create(
    int authenticationExitIntervalSeconds,
    int synchronizationExitIntervalSeconds,
    int runningExitIntervalSeconds,
    int executionExitIntervalSeconds,
    int instructionsExecutionLimit,
    int instructionResultsSendLimit,
    int metricsSendLimit,
    IReadOnlyList<string> allowedCollectors,
    IReadOnlyList<string> allowedInstructions)
  {
    return new Config
    {
      AuthenticationExitIntervalSeconds = authenticationExitIntervalSeconds,
      SynchronizationExitIntervalSeconds = synchronizationExitIntervalSeconds,
      InstructionResultsSendLimit = instructionResultsSendLimit,
      MetricsSendLimit = metricsSendLimit,
      RunningExitIntervalSeconds = runningExitIntervalSeconds,
      ExecutionExitIntervalSeconds = executionExitIntervalSeconds,
      InstructionsExecutionLimit = instructionsExecutionLimit,
      AllowedCollectors = allowedCollectors is { Count: > 0 }
        ? string.Join(",", allowedCollectors)
        : null,
      AllowedInstructions = allowedInstructions is { Count: > 0 }
        ? string.Join(",", allowedInstructions)
        : null,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  public IReadOnlyList<string> GetAllowedCollectorsList()
    => string.IsNullOrEmpty(AllowedCollectors)
      ? []
      : AllowedCollectors.Split(',', StringSplitOptions.RemoveEmptyEntries);

  public IReadOnlyList<string> GetAllowedInstructionsList()
    => string.IsNullOrEmpty(AllowedInstructions)
      ? []
      : AllowedInstructions.Split(',', StringSplitOptions.RemoveEmptyEntries);

  #endregion
}

using System.ComponentModel.DataAnnotations;

namespace WebApi.Domain;

public class Configuration
{
  public long Id { get; init; }

  public long AgentId { get; private init; }

  [Required]
  [MaxLength(200)]
  public string ServerUrl { get; private set; } = null!;

  [Required]
  public int AuthTransitionIntervalSeconds { get; private set; }

  [Required]
  public int ExecutionTransitionIntervalSeconds { get; private set; }

  [Required]
  public int RunningTransitionIntervalSeconds { get; private set; }

  [Required]
  public int SyncTransitionIntervalSeconds { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }

  #region Navigation properties

  /// <summary>
  /// The associated Agent.
  /// </summary>
  public virtual Agent Agent { get; private set; } = null!;

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new Configuration instance.
  /// </summary>
  public static Configuration Create(
    long agentId,
    string serverUrl,
    int authTransitionIntervalSeconds,
    int executionTransitionIntervalSeconds,
    int runningTransitionIntervalSeconds,
    int syncTransitionIntervalSeconds)
  {
    return new Configuration
    {
      AgentId = agentId,
      ServerUrl = serverUrl,
      AuthTransitionIntervalSeconds = authTransitionIntervalSeconds,
      ExecutionTransitionIntervalSeconds = executionTransitionIntervalSeconds,
      RunningTransitionIntervalSeconds = runningTransitionIntervalSeconds,
      SyncTransitionIntervalSeconds = syncTransitionIntervalSeconds,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Patch updates the configuration.
  /// </summary>
  public void Update(
    string? serverUrl = null,
    int? authTransitionIntervalSeconds = null,
    int? executionTransitionIntervalSeconds = null,
    int? runningTransitionIntervalSeconds = null,
    int? syncTransitionIntervalSeconds = null)
  {
    ServerUrl = serverUrl ?? ServerUrl;
    AuthTransitionIntervalSeconds = authTransitionIntervalSeconds ?? AuthTransitionIntervalSeconds;
    ExecutionTransitionIntervalSeconds = executionTransitionIntervalSeconds ?? ExecutionTransitionIntervalSeconds;
    RunningTransitionIntervalSeconds = runningTransitionIntervalSeconds ?? RunningTransitionIntervalSeconds;
    SyncTransitionIntervalSeconds = syncTransitionIntervalSeconds ?? SyncTransitionIntervalSeconds;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  #endregion
}

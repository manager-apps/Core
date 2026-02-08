using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

public enum AgentState
{
  Inactive = 1,
  Active
}

[Index(nameof(Name), IsUnique = true)]
public class Agent
{
  public long Id { get; init; }

  [Required]
  [MaxLength(200)]
  public string Name { get; private init; } = null!;

  [Required]
  [MaxLength(100)]
  public string SourceTag { get; private set; } = null!;

  [Required]
  [MaxLength(100)]
  public string CurrentTag { get; private set; } = null!;

  [Required]
  [MaxLength(50)]
  public string Version { get; private set; } = null!;

  [Required]
  public AgentState State { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }
  public DateTimeOffset LastSeenAt { get; private set; }

  #region Navigation properties

  /// <summary>
  /// Navigation property to hardware information
  /// </summary>
  public virtual Hardware? Hardware { get; private set; }

  /// <summary>
  /// Navigation property to configuration overrides
  /// </summary>
  public virtual Config? Config { get; private set; }

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new Agent instance.
  /// </summary>
  public static Agent Create(
    string name,
    string sourceTag)
  {
    return new Agent
    {
      Name = name,
      SourceTag = sourceTag,
      CurrentTag = sourceTag,
      Version = "1.0.0",
      State = AgentState.Active,
      CreatedAt = DateTimeOffset.UtcNow,
      LastSeenAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Assigns configuration to the agent. Config must be created by a factory or repository.
  /// </summary>
  public void AssignConfig(Config config)
  {
    ArgumentNullException.ThrowIfNull(config);
    Config = config;
  }

  /// <summary>
  /// Assigns hardware information to the agent. Hardware must be created by a factory or repository.
  /// </summary>
  public void AssignHardware(Hardware hardware)
  {
    ArgumentNullException.ThrowIfNull(hardware);
    Hardware = hardware;
  }

  /// <summary>
  /// Tries to get the configuration. Returns false if not assigned.
  /// </summary>
  public bool TryGetConfig(out Config? config)
  {
    config = Config;
    return config is not null;
  }

  /// <summary>
  /// Tries to get the hardware information. Returns false if not assigned.
  /// </summary>
  public bool TryGetHardware(out Hardware? hardware)
  {
    hardware = Hardware;
    return hardware is not null;
  }

  /// <summary>
  /// Checks if agent has configuration assigned.
  /// </summary>
  public bool HasConfig() => Config is not null;

  /// <summary>
  /// Checks if agent has hardware information assigned.
  /// </summary>
  public bool HasHardware() => Hardware is not null;

  /// <summary>
  /// Patch update
  /// </summary>
  public void Update(
    string? sourceTag = null,
    AgentState? state = null)
  {
    SourceTag = sourceTag ?? SourceTag;
    State = state ?? State;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Updates the last seen timestamp to the current time.
  /// </summary>
  public void UpdateLastSeen(
    string currentTag,
    string version)
  {
    Version = version;
    CurrentTag = currentTag;
    LastSeenAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Sets the state of the agent.
  /// </summary>
  public void MarkAsActive()
  {
    State = AgentState.Active;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Sets the state of the agent to inactive.
  /// </summary>
  public void MarkAsInactive()
  {
    State = AgentState.Inactive;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  /// <summary>
  /// Determines whether the agent can be authenticated.
  /// </summary>
  public bool CanAuthenticate()
    => State is AgentState.Active;
  #endregion
}

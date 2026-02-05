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
  public byte[] SecretKeyHash { get; private init; } = null!;

  [Required]
  public byte[] SecretKeySalt { get; private init; } = null!;

  [Required]
  public AgentState State { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }
  public DateTimeOffset LastSeenAt { get; private set; }

  #region Navigation properties

  /// <summary>
  /// Navigation property to hardware information
  /// </summary>
  public virtual Hardware Hardware { get; set; } = null!;

  /// <summary>
  /// Navigation property to configuration overrides
  /// </summary>
  public virtual Config Config { get; set; } = null!;

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new Agent instance.
  /// </summary>
  public static Agent Create(
    Config config,
    Hardware hardware,
    string name,
    string sourceTag,
    byte[] secretKeyHash,
    byte[] secretKeySalt)
  {
    return new Agent
    {
      Config = config,
      Hardware = hardware,
      SourceTag = sourceTag,
      CurrentTag = sourceTag,
      Name = name,
      SecretKeyHash = secretKeyHash,
      SecretKeySalt = secretKeySalt,
      State = AgentState.Inactive,
      CreatedAt = DateTimeOffset.UtcNow,
      LastSeenAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

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
    string currentTag)
  {
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

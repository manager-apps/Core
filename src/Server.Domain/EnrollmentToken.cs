using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

/// <summary>
/// Represents a one-time enrollment token for agent certificate provisioning.
/// </summary>
[Index(nameof(AgentName))]
[Index(nameof(TokenHash), IsUnique = true)]
public class EnrollmentToken
{
  public Guid Id { get; init; }

  /// <summary>
  /// SHA-256 hash of the enrollment token.
  /// </summary>
  public byte[] TokenHash { get; private init; } = null!;

  /// <summary>
  /// Salt used for hashing the token.
  /// </summary>
  public byte[] TokenSalt { get; private init; } = null!;

  /// <summary>
  /// The agent name this token is intended for.
  /// </summary>
  [Required]
  [MaxLength(200)]
  public string AgentName { get; private init; } = null!;

  /// <summary>
  /// When the token expires.
  /// </summary>
  public DateTimeOffset ExpiresAt { get; private init; }

  /// <summary>
  /// Whether the token has been used.
  /// </summary>
  public bool IsUsed { get; private set; }

  /// <summary>
  /// When the token was used.
  /// </summary>
  public DateTimeOffset? UsedAt { get; private set; }

  /// <summary>
  /// Reference to the agent that used this token (if any).
  /// </summary>
  public long? AgentId { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }

  #region Navigation properties

  /// <summary>
  /// Navigation property to the agent.
  /// </summary>
  public virtual Agent? Agent { get; init; }

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new EnrollmentToken instance.
  /// </summary>
  public static EnrollmentToken Create(
    string agentName,
    byte[] tokenHash,
    byte[] tokenSalt,
    TimeSpan validity)
  {
    return new EnrollmentToken
    {
      Id = Guid.NewGuid(),
      AgentName = agentName,
      TokenHash = tokenHash,
      TokenSalt = tokenSalt,
      ExpiresAt = DateTimeOffset.UtcNow.Add(validity),
      IsUsed = false,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Marks the token as used by the specified agent.
  /// </summary>
  public void MarkAsUsed(long agentId)
  {
    IsUsed = true;
    UsedAt = DateTimeOffset.UtcNow;
    AgentId = agentId;
  }

  /// <summary>
  /// Checks if the token is valid (not expired and not used).
  /// </summary>
  public bool IsValid()
  {
    return !IsUsed && ExpiresAt > DateTimeOffset.UtcNow;
  }

  #endregion
}

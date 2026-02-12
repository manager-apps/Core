using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

/// <summary>
/// Represents a client certificate issued to an agent for mTLS authentication.
/// </summary>
[Index(nameof(SerialNumber), IsUnique = true)]
[Index(nameof(Thumbprint), IsUnique = true)]
public class Certificate
{
  public long Id { get; init; }

  /// <summary>
  /// Certificate serial number (hex string).
  /// </summary>
  [Required]
  [MaxLength(64)]
  public string SerialNumber { get; private init; } = null!;

  /// <summary>
  /// SHA-256 thumbprint of the certificate.
  /// </summary>
  [Required]
  [MaxLength(64)]
  public string Thumbprint { get; private init; } = null!;

  /// <summary>
  /// Subject name (CN) of the certificate.
  /// </summary>
  [Required]
  [MaxLength(256)]
  public string SubjectName { get; private init; } = null!;

  /// <summary>
  /// Reference to the agent this certificate belongs to.
  /// </summary>
  public long AgentId { get; private init; }

  /// <summary>
  /// When the certificate was issued.
  /// </summary>
  public DateTimeOffset IssuedAt { get; private init; }

  /// <summary>
  /// When the certificate expires.
  /// </summary>
  public DateTimeOffset ExpiresAt { get; private init; }

  /// <summary>
  /// When the certificate was revoked (null if not revoked).
  /// </summary>
  public DateTimeOffset? RevokedAt { get; private set; }

  /// <summary>
  /// Reason for revocation (null if not revoked).
  /// </summary>
  [MaxLength(256)]
  public string? RevocationReason { get; private set; }

  /// <summary>
  /// Indicates whether this is the currently active certificate for the agent.
  /// </summary>
  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }

  #region Navigation properties

  /// <summary>
  /// Navigation property to the agent.
  /// </summary>
  public virtual Agent Agent { get; init; } = null!;

  #endregion

  #region Factory methods

  /// <summary>
  /// Creates a new Certificate instance.
  /// </summary>
  public static Certificate Create(
    long agentId,
    string serialNumber,
    string thumbprint,
    string subjectName,
    DateTimeOffset issuedAt,
    DateTimeOffset expiresAt)
  {
    return new Certificate
    {
      AgentId = agentId,
      SerialNumber = serialNumber,
      Thumbprint = thumbprint,
      SubjectName = subjectName,
      IssuedAt = issuedAt,
      ExpiresAt = expiresAt,
      IsActive = true,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  /// <summary>
  /// Revokes the certificate.
  /// </summary>
  public void Revoke(string reason)
  {
    RevokedAt = DateTimeOffset.UtcNow;
    RevocationReason = reason;
    IsActive = false;
  }

  /// <summary>
  /// Deactivates the certificate (e.g., when replaced by a new one).
  /// </summary>
  public void Deactivate()
  {
    IsActive = false;
  }

  /// <summary>
  /// Checks if the certificate is valid (not expired and not revoked).
  /// </summary>
  public bool IsValid()
  {
    return RevokedAt is null
           && ExpiresAt > DateTimeOffset.UtcNow
           && IsActive;
  }

  /// <summary>
  /// Checks if the certificate needs renewal (within threshold days of expiry).
  /// </summary>
  public bool NeedsRenewal(int thresholdDays = 30)
  {
    return ExpiresAt <= DateTimeOffset.UtcNow.AddDays(thresholdDays);
  }

  #endregion
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

[Index(nameof(AgentId), IsUnique = true)]
public class Hardware
{
  public long Id { get; init; }

  [Required]
  public long AgentId { get; init; }

  [MaxLength(200)]
  public string? OsVersion { get; private set; }

  [MaxLength(200)]
  public string? MachineName { get; private set; }

  [Required]
  public int ProcessorCount { get; private set; }

  [Required]
  public long TotalMemoryBytes { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }
  public DateTimeOffset? UpdatedAt { get; private set; }

  #region Navigation properties

  /// <summary>
  /// Navigation property to the associated agent. This is a one-to-one
  /// relationship where each agent has one hardware record.
  /// </summary>
  public virtual Agent Agent { get; init; } = null!;

  #endregion

  #region Factory methods

  public static Hardware Create(
    string osVersion,
    string machineName,
    int processorCount,
    long totalMemoryBytes)
  {
    return new Hardware
    {
      OsVersion = osVersion,
      MachineName = machineName,
      ProcessorCount = processorCount,
      TotalMemoryBytes = totalMemoryBytes,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion

  #region Domain methods

  public void Update(
    string? osVersion,
    string? machineName,
    int? processorCount,
    long? totalMemoryBytes)
  {
    OsVersion = osVersion ?? OsVersion;
    MachineName = machineName ?? MachineName;
    ProcessorCount = processorCount ?? ProcessorCount;
    TotalMemoryBytes = totalMemoryBytes ?? TotalMemoryBytes;
    UpdatedAt = DateTimeOffset.UtcNow;
  }

  #endregion
}

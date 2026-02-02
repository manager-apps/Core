using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

[Index(nameof(AgentName))]
public class Metric
{
  [Required]
  public string AgentName { get; private init; } = null!;

  [Required]
  public string Name { get; private init; } = null!;

  [Required]
  public string Type { get; private init; } = null!;

  [Required]
  public double Value { get; private init; }

  [Required]
  public string Unit { get; private init; } = null!;

  [Required]
  public DateTimeOffset TimestampUtc { get; private init; }

  [MaxLength(4000)]
  public string? MetadataJson { get; private init; }

  public DateTimeOffset CreatedAt { get; private init; }

  #region Factory methods

  /// <summary>
  /// Creates a new Metric instance.
  /// </summary>
  public static Metric Create(
    string agentName,
    string name,
    string type,
    double value,
    string unit,
    DateTimeOffset timestampUtc,
    string? metadataJson = null)
  {
    return new Metric
    {
      AgentName = agentName,
      Name = name,
      Type = type,
      Value = value,
      Unit = unit,
      TimestampUtc = timestampUtc,
      MetadataJson = metadataJson,
      CreatedAt = DateTimeOffset.UtcNow
    };
  }

  #endregion
}

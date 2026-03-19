using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Server.Domain;

[Index(nameof(GoogleId), IsUnique = true)]
public class User
{
  public long Id { get; private init; }

  [Required]
  [MaxLength(100)]
  public string GoogleId { get; private init; } = null!;

  [Required]
  [MaxLength(200)]
  public string Email { get; private init; } = null!;

  [Required]
  [MaxLength(200)]
  public string Name { get; private set; } = null!;

  [MaxLength(500)]
  public string? AvatarUrl { get; private set; }

  public DateTimeOffset CreatedAt { get; private init; }

  public static User Create(string googleId, string email, string name, string? avatarUrl) =>
    new() { GoogleId = googleId, Email = email, Name = name, AvatarUrl = avatarUrl, CreatedAt = DateTimeOffset.UtcNow };
}

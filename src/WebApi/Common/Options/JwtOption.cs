namespace WebApi.Common.Options;

public class JwtOption
{
  /// <summary>
  /// The secret key used for signing JWT tokens.
  /// </summary>
  public string Secret { get; init; } = null!;

  /// <summary>
  /// The issuer of the JWT tokens.
  /// </summary>
  public string Issuer { get; init; } = null!;

  /// <summary>
  /// The audience for the JWT tokens.
  /// </summary>
  public string Audience { get; init; } = null!;

  /// <summary>
  /// The expiration time in minutes for the JWT tokens.
  /// </summary>
  public int ExpiryMinutes { get; init; }
}

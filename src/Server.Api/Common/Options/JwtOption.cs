namespace Server.Api.Common.Options;

public class JwtOption
{
  public string Secret { get; init; } = null!;
  public string Issuer { get; init; } = null!;
  public string Audience { get; init; } = null!;
  public int ExpiryMinutes { get; init; }
}

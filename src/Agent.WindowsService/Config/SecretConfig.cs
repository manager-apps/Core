namespace Agent.WindowsService.Config;

public static class SecretConfig
{
  /// <summary>
  /// Key for storing the authentication token.
  /// </summary>
  public static readonly string AuthTokenKey = "auth.token";

  /// <summary>
  /// Key for storing the refresh token.
  /// </summary>
  public static readonly string RefreshTokenKey = "auth.refresh-token";

  /// <summary>
  /// Key for storing the client secret.
  /// </summary>
  public static readonly string ClientSecretKey = "auth.client-secret";

  /// <summary>
  /// Entropy used for data protection.
  /// </summary>
  public static readonly byte[] Entropy = "Agent.WindowsService.Secrets.v1"u8.ToArray();
}

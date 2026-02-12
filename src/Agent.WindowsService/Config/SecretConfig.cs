namespace Agent.WindowsService.Config;

public static class SecretConfig
{
  /// <summary>
  /// Entropy used for certificate protection (DPAPI).
  /// </summary>
  public static readonly byte[] CertificateEntropy
    = "Agent.WindowsService.Certificates.v1"u8.ToArray();
}

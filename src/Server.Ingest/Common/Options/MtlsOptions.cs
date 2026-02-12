namespace Server.Ingest.Common.Options;

public class MtlsOptions
{
  public const string SectionName = "Mtls";
  public string? CaPath { get; set; }
  public string? CaKeyPath { get; set; }
  public string? CaPassword { get; set; }
  public string? CaCommonName { get; set; } = "Manager Internal CA";
  public string? ServerCertificatePath { get; set; }
  public string? ServerCertificatePassword { get; set; }
  public int MtlsPort { get; set; } = 8443;
  public int CertificateValidityDays { get; set; } = 365;
  public int RenewalThresholdDays { get; set; } = 30;
}

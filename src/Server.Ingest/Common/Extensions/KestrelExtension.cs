using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Server.Ingest.Common.Options;

namespace Server.Ingest.Common.Extensions;

/// <summary>
/// Extension methods for Kestrel configuration with mTLS support.
/// </summary>
public static class KestrelExtension
{
  public static void ConfigureKestrelWithMtls(this WebApplicationBuilder builder)
  {
    var mtlsOptions = builder.Configuration
      .GetSection(MtlsOptions.SectionName)
      .Get<MtlsOptions>()!;

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
      serverOptions.ListenAnyIP(5140, listenOptions =>
      {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
      });
      serverOptions.ListenAnyIP(mtlsOptions.MtlsPort, listenOptions =>
      {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(httpsOptions =>
        {
          ConfigureHttpsOptions(httpsOptions, mtlsOptions);
        });
      });
    });
  }

  private static void ConfigureHttpsOptions(
    HttpsConnectionAdapterOptions httpsOptions,
    MtlsOptions mtlsOptions)
  {
    httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
    httpsOptions.CheckCertificateRevocation = false;

    if (!string.IsNullOrEmpty(mtlsOptions.ServerCertificatePath) &&
        File.Exists(mtlsOptions.ServerCertificatePath))
    {
      httpsOptions.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile(
        mtlsOptions.ServerCertificatePath,
        mtlsOptions.ServerCertificatePassword);
    }
    else
    {
      throw new InvalidOperationException(
        "Server certificate must be configured for mTLS in production. " +
        "Set Mtls:ServerCertificatePath in configuration.");
    }

    httpsOptions.ClientCertificateValidation = (_,_,_) => true;
  }
}

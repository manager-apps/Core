using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Options;
using Server.Ingest.Features.Cert;
using Server.Ingest.Features.Cert.Enroll;
using Server.Ingest.Features.Cert.Renew;
using Server.Ingest.Features.Cert.Revocation;
using Server.Ingest.Features.Cert.Status;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Common.Extensions;

/// <summary>
/// Extension methods for mTLS service registration.
/// </summary>
public static class MtlsExtension
{
    public static void AddMtls(this IServiceCollection services,
      IConfiguration configuration)
    {
        var mtlsSection = configuration.GetSection(MtlsOptions.SectionName);
        services.Configure<MtlsOptions>(mtlsSection);
        services.AddSingleton<ICaAuthority, CaAuthority>();
        services.AddScoped<IDataHasher, HmacDataHasher>();
    }
}

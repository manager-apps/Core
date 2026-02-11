using Server.Ingest.Common.Interfaces;
using Server.Ingest.Common.Options;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Common.Extensions;

internal static class MtlsExtension
{
    internal static void AddMtls(
      this IServiceCollection services,
      IConfiguration configuration)
    {
        var mtlsSection = configuration.GetSection(MtlsOptions.SectionName);
        services.Configure<MtlsOptions>(mtlsSection);
        services.AddSingleton<ICaAuthority, CaAuthority>();
        services.AddScoped<IDataHasher, HmacDataHasher>();
    }
}

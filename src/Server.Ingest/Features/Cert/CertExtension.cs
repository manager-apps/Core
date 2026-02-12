using Server.Ingest.Common.Interfaces;
using Server.Ingest.Features.Cert.Ca;
using Server.Ingest.Features.Cert.Enroll;
using Server.Ingest.Features.Cert.Renew;
using Server.Ingest.Features.Cert.Revocation;
using Server.Ingest.Features.Cert.Status;

namespace Server.Ingest.Features.Cert;

public static class CertExtension
{
    public static void AddCertServices(this IServiceCollection services)
    {
        services.AddScoped<ICertService, CertService>();
        services.AddScoped<ICertEnrollHandler, CertEnrollHandler>();
        services.AddScoped<ICertRenewHandler, CertRenewHandler>();
        services.AddScoped<ICertStatusHandler, CertStatusHandler>();
        services.AddScoped<ICertRevocationHandler, CertRevocationHandler>();
    }

    public static void MapCertEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetCaEndpoint();
        app.MapEnrollWithToken();
        app.MapCertRenewEndpoint();
        app.MapCertStatusEndpoint();
        app.MapCertRevocationEndpoint();
    }
}

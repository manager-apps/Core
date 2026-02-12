using Server.Api.Features.Cert.Create;

namespace Server.Api.Features.Cert;

internal static class CertificateExtension
{
  internal static void AddCertServices(this IServiceCollection services)
  {
    services.AddScoped<IEnrollmentTokenCreateHandler, EnrollmentTokenCreateHandler>();
  }

  internal static void MapCertEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/certs");

    group.MapCreateEnrollmentTokenEndpoint();
  }
}

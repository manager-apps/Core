using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Interfaces;

namespace Server.Ingest.Features.Cert.Ca;

internal static class GetCaEndpoint
{
    internal static void MapGetCaEndpoint(this IEndpointRouteBuilder app)
      => app.MapGet("certificates/ca", (
            [FromServices] ICaAuthority caAuthority)
            => Results.Text(caAuthority.GetCaCertificatePem(), "application/x-pem-file"))
        .Produces<string>(StatusCodes.Status200OK, "application/x-pem-file");
}

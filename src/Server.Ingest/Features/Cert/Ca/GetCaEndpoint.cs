using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Interfaces;

namespace Server.Ingest.Features.Cert.Ca;

internal static class GetCaEndpoint
{
    private const string Tag = "Certificate";

    internal static void MapGetCaEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/agents/certificates/ca", (
            [FromServices] ICaAuthority caAuthority) =>
        {
            var caCertPem = caAuthority.GetCaCertificatePem();
            return Results.Text(caCertPem, "application/x-pem-file");
        })
        .WithName("GetCaCertificate")
        .WithTags(Tag)
        .Produces<string>(StatusCodes.Status200OK, "application/x-pem-file");
    }
}

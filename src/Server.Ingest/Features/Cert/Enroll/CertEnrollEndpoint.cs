using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Enroll;

internal static class CertEnrollEndpoint
{
    private const string Tag = "Certificate";

    internal static void MapCertEnrollEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapEnrollWithToken();
    }

    private static void MapEnrollWithToken(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/agents/certificates/enroll/token", async (
            [FromServices] ICertEnrollHandler handler,
            [FromBody] TokenEnrollRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.EnrollWithTokenAsync(request, cancellationToken);
            return result.ToApiResult(createdUri: "/api/v1/agents/certificates/status");
        })
        .WithName("EnrollCertificateWithToken")
        .WithTags(Tag)
        .Produces<CertEnrollResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Renew;

internal static class CertRenewEndpoint
{
    private const string Tag = "Certificate";

    internal static void MapCertRenewEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/agents/certificates/renew", async (
            [FromServices] ICertRenewHandler handler,
            [FromBody] CertRenewRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var agentName = user.Identity?.Name;
            if (string.IsNullOrEmpty(agentName))
                return Results.Unauthorized();

            var result = await handler.RenewAsync(agentName, request, cancellationToken);
            return result.ToApiResult(createdUri: "/api/v1/agents/certificates/status");
        })
        .RequireAuthorization()
        .WithName("RenewCertificate")
        .WithTags(Tag)
        .Produces<CertEnrollResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

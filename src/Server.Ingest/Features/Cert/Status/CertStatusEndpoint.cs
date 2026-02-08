using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Status;

internal static class CertStatusEndpoint
{
    private const string Tag = "Certificate";

    internal static void MapCertStatusEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/agents/certificates/status", async (
            [FromServices] ICertStatusHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var agentName = user.Identity?.Name;
            if (string.IsNullOrEmpty(agentName))
                return Results.Unauthorized();

            var result = await handler.GetStatusAsync(agentName, cancellationToken);
            return result.ToApiResult();
        })
        .RequireAuthorization()
        .WithName("GetCertificateStatus")
        .WithTags(Tag)
        .Produces<CertStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}

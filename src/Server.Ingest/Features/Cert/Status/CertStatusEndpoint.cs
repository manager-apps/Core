using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Status;

internal static class CertStatusEndpoint
{
    internal static void MapCertStatusEndpoint(this IEndpointRouteBuilder app)
      => app.MapGet("certificates/status", async (
            [FromServices] ICertStatusHandler handler,
            HttpContext context,
            CancellationToken cancellationToken)
            => (await handler.GetStatusAsync(context.User, cancellationToken))
              .ToApiResult())
        .RequireAuthorization()
        .Produces<CertStatusResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .MapToApiVersion(ApiVersioningExtension.V1);
}

using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Enroll;

internal static class CertEnrollEndpoint
{
    internal static void MapEnrollWithToken(this IEndpointRouteBuilder app)
      => app.MapPost("certificates/enroll/token", async (
            [FromServices] ICertEnrollHandler handler,
            [FromBody] TokenEnrollRequest request,
            CancellationToken cancellationToken)
            => (await handler.EnrollWithTokenAsync(request, cancellationToken))
              .ToApiResult(createdUri: "certificates/status"))
        .Produces<CertEnrollResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .MapToApiVersion(ApiVersioningExtension.V1);
}

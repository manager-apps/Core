using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;
using Microsoft.AspNetCore.Mvc;

namespace Server.Ingest.Features.Cert.Renew;

internal static class CertRenewEndpoint
{
  internal static void MapCertRenewEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("certificates/renew", async (
          [FromServices] ICertRenewHandler handler,
          [FromBody] CertRenewRequest request,
          HttpContext context,
          CancellationToken cancellationToken)
            => (await handler.RenewAsync(context.User, request, cancellationToken))
              .ToApiResult(createdUri: "/certificates/status"))
      .RequireAuthorization()
      .Produces<CertEnrollResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

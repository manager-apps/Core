using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Revocation;

internal static class CertRevocationEndpoint
{
  internal static void MapCertRevocationEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("certificates/revocation/{thumbprint}", async (
        [FromRoute] string thumbprint,
        [FromServices] ICertRevocationHandler handler,
        CancellationToken ct)
        => (await handler.CheckRevocationAsync(thumbprint, ct))
          .ToApiResult())
      .AllowAnonymous()
      .Produces<CertRevocationResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

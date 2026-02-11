using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;

namespace Server.Api.Features.Agent.Cert.Revoke;

internal static class CertRevokeEndpoint
{
  internal static void MapRevokeCertificateEndpoint(this IEndpointRouteBuilder app)
  {
    app.MapPost("/{agentId:long}/certs/revoke", async (
      [FromRoute] long agentId,
      [FromBody] RevokeRequest request,
      [FromServices] ICertRevokeHandler handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(agentId, request.Reason, cancellationToken);
      return result.ToApiResult();
    })
    .Produces<bool>()
    .ProducesProblem(StatusCodes.Status404NotFound)
    .MapToApiVersion(ApiVersioningExtension.V1);
  }
}

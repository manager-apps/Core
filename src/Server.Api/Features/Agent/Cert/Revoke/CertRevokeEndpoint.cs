using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;

namespace Server.Api.Features.Agent.Cert.Revoke;

public static class CertRevokeEndpoint
{
  public static void MapRevokeCertificateEndpoint(this IEndpointRouteBuilder app)
  {
    app.MapPost("/{agentId:long}/revoke", async (
      [FromRoute] long agentId,
      [FromBody] RevokeRequest request,
      [FromServices] ICertRevokeHandler handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(agentId, request.Reason, cancellationToken);
      return result.ToApiResult();
    })
    .Produces<bool>()
    .ProducesProblem(StatusCodes.Status404NotFound);
  }
}

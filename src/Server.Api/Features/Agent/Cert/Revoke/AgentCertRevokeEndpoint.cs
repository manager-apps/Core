using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;

namespace Server.Api.Features.Agent.Cert.Revoke;

internal static class AgentCertRevokeEndpoint
{
  internal static void MapRevokeCertificateEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("/{agentId:long}/certs/revoke", async (
        [FromRoute] long agentId,
        [FromBody] RevokeRequest request,
        [FromServices] IAgentCertRevokeHandler handler,
        CancellationToken cancellationToken)
        => (await handler.HandleAsync(agentId, request, cancellationToken)) .ToApiResult())
      .Produces<bool>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

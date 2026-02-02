using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.Status;

internal static class UpdateStateEndpoint
{
  internal static void MapUpdateStatusEndpoint(this IEndpointRouteBuilder app)
    => app.MapPut("/{id:long}/status",
        async (
          [FromRoute] long id,
          [FromServices] IUpdateStatusHandler handler,
          [FromBody] AgentUpdateStateRequest request,
          CancellationToken ct)
          => (await handler.HandleAsync(id, request, ct)).ToApiResult())
      .Produces<AgentResponse>();
}

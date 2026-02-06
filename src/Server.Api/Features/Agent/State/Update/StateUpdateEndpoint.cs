using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.State.Update;

internal static class StateUpdateEndpoint
{
  internal static void MapUpdateStateEndpoint(this IEndpointRouteBuilder app)
    => app.MapPut("/{agentId:long}/state",
        async (
            [FromRoute] long agentId,
            [FromServices] IStateUpdateHandler handler,
            [FromBody] AgentUpdateStateRequest request,
            CancellationToken ct)
          => (await handler.HandleAsync(agentId, request, ct)).ToApiResult())
      .WithTags("User")
      .Produces<AgentResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

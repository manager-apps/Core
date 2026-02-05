using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.GetById;

internal static class AgentGetByIdEndpoint
{
  internal static void MapGetByIdAgentEndpoint(this IEndpointRouteBuilder app)
   => app.MapGet("/{id:long}",
        async (
            [FromRoute] long id,
            [FromServices] IAgentGetByIdHandler handler,
            CancellationToken ct)
          => (await handler.HandleAsync(id, ct)).ToApiResult())
      .Produces<AgentDetailResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound);
}

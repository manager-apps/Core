using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.GetById;

internal static class GetByIdAgentEndpoint
{
  internal static void MapGetByIdAgentEndpoint(this IEndpointRouteBuilder app)
   => app.MapGet("/{id:long}",
        async (
            [FromRoute] long id,
            [FromServices] IGetByIdAgentHandler handler,
            CancellationToken ct)
          => (await handler.HandleAsync(id, ct)).ToApiResult())
      .Produces<AgentResponse>();
}

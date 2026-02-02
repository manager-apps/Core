using Microsoft.AspNetCore.Mvc;

namespace Server.Api.Features.Agent.GetAll;

internal static class GetAllAgentsEndpoint
{
  internal static void MapGetAllAgentsEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("/",
        async (
          [FromServices] IGetAllAgentsHandler handler,
          CancellationToken ct)
          => await handler.HandleAsync(ct))
      .Produces<IEnumerable<AgentResponse>>();
}

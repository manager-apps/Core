using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;

namespace Server.Api.Features.Agent.GetAll;

internal static class AgentsGetAllEndpoint
{
  internal static void MapGetAllAgentsEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("/", async (
        [FromServices] IAgentGetAllHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(ct))
      .Produces<IEnumerable<AgentResponse>>()
      .MapToApiVersion(ApiVersioningExtension.V1);
}

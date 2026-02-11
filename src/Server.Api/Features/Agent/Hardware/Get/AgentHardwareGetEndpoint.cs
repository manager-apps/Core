using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Hardware;

namespace Server.Api.Features.Agent.Hardware.Get;

internal static class AgentHardwareGetEndpoint
{
  internal static void MapHardwareGetEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("{agentId:long}/hardware", async (
        [FromRoute] long agentId,
        [FromServices] IAgentHardwareGetHandler handler,
        CancellationToken ct) =>
        (await handler.HandleAsync(agentId, ct)).ToApiResult())
      .Produces<HardwareResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Hardware;

namespace Server.Api.Features.Agent.Hardware.Get;

internal static class HardwareGetEndpoint
{
  internal static void MapHardwareGetEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("{agentId:long}/hardware", async (
        [FromRoute] long agentId,
        [FromServices] IHardwareGetHandler handler,
        CancellationToken ct) =>
        (await handler.HandleAsync(agentId, ct)).ToApiResult())
      .Produces<HardwareResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

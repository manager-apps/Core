using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Config;

namespace Server.Api.Features.Agent.Config.Get;

internal static class ConfigGetEndpoint
{
  internal static void MapConfigGetEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("{agentId}/config", async (
      [FromRoute] long agentId,
      [FromServices] IConfigGetHandler handler,
      CancellationToken ct) =>
    {
      var result = await handler.HandleAsync(agentId, ct);
      return result.ToApiResult();
    })
    .WithTags("User")
    .Produces<ConfigResponse>()
    .ProducesProblem(StatusCodes.Status404NotFound)
    .MapToApiVersion(ApiVersioningExtension.V1);
}

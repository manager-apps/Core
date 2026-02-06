using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;
using Server.Api.Features.Config;

namespace Server.Api.Features.Agent.Config.Update;

internal static class ConfigUpdateEndpoint
{
  internal static void MapConfigUpdateEndpoint(this IEndpointRouteBuilder app)
    => app.MapPut("{agentId}/config", async (
        [FromRoute] long agentId,
        [FromBody] ConfigUpdateRequest request,
        [FromServices] IConfigUpdateHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleAsync(agentId, request, ct);
        return result.ToApiResult();
      })
      .WithTags("User")
      .Produces<ConfigResponse>()
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

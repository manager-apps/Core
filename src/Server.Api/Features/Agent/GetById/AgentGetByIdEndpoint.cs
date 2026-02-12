using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.GetById;

internal static class AgentGetByIdEndpoint
{
  internal static void MapGetByIdAgentEndpoint(this IEndpointRouteBuilder app)
   => app.MapGet("/{agentId:long}", async (
          [FromRoute] long agentId,
          [FromServices] IAgentGetByIdHandler handler,
          CancellationToken ct)
          => (await handler.HandleAsync(agentId, ct)).ToApiResult())
      .Produces<AgentDetailResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

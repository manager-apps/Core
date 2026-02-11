using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Features.Instruction;

namespace Server.Api.Features.Agent.Instruction.GetAll;

internal static class InstructionsGetAllEndpoint
{
  internal static void MapGetAllInstructionsEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("{agentId:long}/instructions", async (
      [FromRoute] long agentId,
      [FromServices] IInstructionsGetAllHandler handler,
      CancellationToken cancellationToken) =>
    {
      var instructions = await handler.HandleAsync(agentId, cancellationToken);
      return Results.Ok(instructions);
    })
    .Produces<IEnumerable<InstructionResponse>>()
    .MapToApiVersion(ApiVersioningExtension.V1);
}

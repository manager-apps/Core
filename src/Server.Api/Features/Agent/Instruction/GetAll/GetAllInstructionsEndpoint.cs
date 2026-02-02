using Microsoft.AspNetCore.Mvc;
using Server.Api.Features.Instruction;

namespace Server.Api.Features.Agent.Instruction.GetAll;

internal static class GetAllInstructionsEndpoint
{
  internal static void MapGetAllInstructionsEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("{id:long}/instructions", async (
      long id,
      [FromServices] IGetAllInstructionsHandler handler,
      CancellationToken cancellationToken) =>
    {
      var instructions = await handler.HandleAsync(id, cancellationToken);
      return Results.Ok(instructions);
    })
    .WithTags("Agent")
    .Produces<IEnumerable<InstructionResponse>>();
}

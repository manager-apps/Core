using Microsoft.AspNetCore.Mvc;

namespace WebApi.Features.Instruction.GetAll;

internal static class GetAllInstructionsEndpoint
{
  internal static void MapGetAllInstructionsEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("/", async (
      [FromServices] IGetAllInstructionsHandler handler,
      CancellationToken cancellationToken) =>
    {
      var instructions = await handler.HandleAsync(cancellationToken);
      return Results.Ok(instructions);
    })
    .WithTags("Instructions")
    .WithName("GetAll")
    .Produces<IEnumerable<InstructionResponse>>();
}

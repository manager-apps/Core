using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction.Create;

internal static class CreateInstructionEndpoint
{
  internal static void MapCreateInstructionEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("/", async (
        [FromBody] CreateInstructionRequest request,
        [FromServices] ICreateInstructionHandler createInstructionUseCase,
        CancellationToken ct) =>
      {
        var result = await createInstructionUseCase.HandleAsync(request, ct);
        return result.ToApiResult(
          createdUri: $"/instructions/{result.Value.Id}");
      })
      .WithTags("Instructions")
      .WithName("Create")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest);
}

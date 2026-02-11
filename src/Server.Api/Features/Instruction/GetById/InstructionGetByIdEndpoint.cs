using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction.GetById;

internal static class InstructionGetByIdEndpoint
{
  internal static void MapInstructionGetByIdEndpoint(this IEndpointRouteBuilder app)
    => app.MapGet("/{instructionId:long}", async (
        [FromRoute] long instructionId,
        [FromServices] IInstructionGetByIdHandler handler,
        CancellationToken ct)
        => (await handler.HandleAsync(instructionId, ct)).ToApiResult())
      .Produces<InstructionResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

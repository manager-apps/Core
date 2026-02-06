using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Features.Instruction;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.Instruction.Create;

internal static class InstructionCreateEndpoint
{
  internal static void MapCreateInstructionEndpoint(this IEndpointRouteBuilder app)
  {
    app.MapPost("{agentId}/instructions/shell", async (
        [FromRoute] long agentId,
        [FromBody] CreateShellCommandRequest request,
        [FromServices] IInstructionCreateHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleShellCommandAsync(agentId, request, ct);
        return result.ToApiResult(
          createdUri: $"/instructions/{result.Value?.Id}");
      })
      .WithTags("User")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);

    app.MapPost("{agentId}/instructions/gpo", async (
        [FromRoute] long agentId,
        [FromBody] CreateGpoSetRequest request,
        [FromServices] IInstructionCreateHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleGpoSetAsync(agentId, request, ct);
        return result.ToApiResult(
          createdUri: $"/instructions/{result.Value?.Id}");
      })
      .WithTags("User")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
  }
}

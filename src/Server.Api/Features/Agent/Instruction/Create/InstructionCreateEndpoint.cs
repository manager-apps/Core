using Microsoft.AspNetCore.Mvc;
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
      .WithName("CreateShellCommandInstruction")
      .WithSummary("Create a shell command instruction for a specific agent.")
      .WithDescription("Creates a new shell command instruction to be executed on the agent.")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound);

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
      .WithName("CreateGpoSetInstruction")
      .WithSummary("Create a GPO set instruction for a specific agent.")
      .WithDescription("Creates a new Group Policy Object setting instruction for the agent.")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound);
  }
}

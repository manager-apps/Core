using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Result;
using WebApi.Features.Instruction;

namespace WebApi.Features.Agent.Instruction.Create;

public static class CreateInstructionEndpoint
{
  public static void MapCreateInstructionEndpoint(this IEndpointRouteBuilder app)
   => app.MapPost("{agentId}/instruction", async (
      [FromRoute] long agentId,
      [FromBody] CreateAgentInstructionRequest request,
      [FromServices] ICreateInstructionHandler handler,
      CancellationToken ct) =>
    {
      var result = await handler.HandleAsync(agentId, request, ct);
      return result.ToApiResult(
        createdUri: $"/instructions/{result.Value.Id}");
    })
    .WithName("CreateAgentInstruction")
    .WithSummary("Create a new instruction for a specific agent.")
    .WithDescription("Creates a new instruction associated with the given agent ID.")
    .Produces<InstructionResponse>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);
}

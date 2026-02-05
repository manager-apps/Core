using Microsoft.AspNetCore.Mvc;
using Server.Api.Features.Instruction;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.Instruction.Create;

public static class CreateInstructionEndpoint
{
  public static void MapCreateInstructionEndpoint(this IEndpointRouteBuilder app)
  {
    app.MapPost("{agentId}/instructions", async (
        [FromRoute] long agentId,
        [FromBody] CreateAgentInstructionRequest request,
        [FromServices] ICreateInstructionHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleAsync(agentId, request, ct);
        return result.ToApiResult(
          createdUri: $"/instructions/{result.Value?.Id}");
      })
      .WithName("CreateAgentInstruction")
      .WithSummary("Create a new instruction for a specific agent.")
      .WithDescription("Creates a new instruction associated with the given agent ID. Accepts raw payload JSON.")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound);

    app.MapPost("{agentId}/instructions/shell", async (
        [FromRoute] long agentId,
        [FromBody] CreateShellCommandRequest request,
        [FromServices] ICreateInstructionHandler handler,
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
        [FromServices] ICreateInstructionHandler handler,
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

    app.MapPost("{agentId}/instructions/config", async (
        [FromRoute] long agentId,
        [FromBody] CreateConfigSyncRequest request,
        [FromServices] ICreateInstructionHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleConfigSyncAsync(agentId, request, ct);
        return result.ToApiResult(
          createdUri: $"/instructions/{result.Value?.Id}");
      })
      .WithName("CreateConfigSyncInstruction")
      .WithSummary("Create a config sync instruction for a specific agent.")
      .WithDescription("Creates a configuration sync instruction. Null fields will use current agent config values.")
      .Produces<InstructionResponse>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status404NotFound);
  }
}

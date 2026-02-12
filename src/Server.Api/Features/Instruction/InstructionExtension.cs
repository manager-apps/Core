using Server.Api.Features.Instruction.GetById;

namespace Server.Api.Features.Instruction;

internal static class InstructionExtension
{
  internal static void AddInstructionServices(this IServiceCollection services)
  {
    services.AddScoped<IInstructionGetByIdHandler, InstructionGetByIdHandler>();
  }

  internal static void MapInstructionEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/instructions");

    group.MapInstructionGetByIdEndpoint();
  }
}

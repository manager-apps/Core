using WebApi.Features.Instruction.Create;
using WebApi.Features.Instruction.GetAll;

namespace WebApi.Features.Instruction;

public static class InstructionExtension
{
  public static void AddInstructionServices(this IServiceCollection services)
  {
    services.AddScoped<ICreateInstructionHandler, CreateInstructionHandler>();
    services.AddScoped<IGetAllInstructionsHandler, GetAllInstructionsHandler>();
  }

  public static void MapInstructionEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/instructions")
      .WithTags("Instruction");

    group.MapCreateInstructionEndpoint();
    group.MapGetAllInstructionsEndpoint();
  }
}

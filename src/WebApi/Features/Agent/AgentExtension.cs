using WebApi.Features.Agent.Auth;
using WebApi.Features.Agent.GetAll;
using WebApi.Features.Agent.GetById;
using WebApi.Features.Agent.Instruction.Create;
using WebApi.Features.Agent.Instruction.GetAll;
using WebApi.Features.Agent.Report;
using WebApi.Features.Agent.Status;

namespace WebApi.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentAuthHandler, AgentAuthHandler>();
    services.AddScoped<IAgentReportHandler, AgentReportHandler>();
    services.AddScoped<IGetAllAgentsHandler, GetAllAgentsHandler>();
    services.AddScoped<IUpdateStatusHandler, UpdateStateHandler>();
    services.AddScoped<IGetByIdAgentHandler, GetByIdAgentHandler>();

    services.AddScoped<IGetAllInstructionsHandler, GetAllInstructionsHandler>();
    services.AddScoped<ICreateInstructionHandler, CreateInstructionHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agent")
      .WithTags("Agent");

    group.MapAgentAuthEndpoint();
    group.MapAgentReportEndpoint();
    group.MapGetAllAgentsEndpoint();
    group.MapUpdateStatusEndpoint();
    group.MapGetByIdAgentEndpoint();

    group.MapGetAllInstructionsEndpoint();
    group.MapCreateInstructionEndpoint();
  }
}

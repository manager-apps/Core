using Server.Api.Features.Agent.Auth;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Agent.Report;
using Server.Api.Features.Agent.Status;

namespace Server.Api.Features.Agent;

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

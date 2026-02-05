using Server.Api.Features.Agent.Auth;
using Server.Api.Features.Agent.Config;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Agent.Report;
using Server.Api.Features.Agent.State;

namespace Server.Api.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentAuthHandler, AgentAuthHandler>();
    services.AddScoped<IAgentReportHandler, AgentReportCreateHandler>();
    services.AddScoped<IAgentGetAllHandler, AgentGetAllHandler>();
    services.AddScoped<IUpdateStatusHandler, AgentStateUpdateHandler>();
    services.AddScoped<IAgentGetByIdHandler, AgentGetByIdHandler>();

    services.AddScoped<IInstructionsGetAllHandler, InstructionsGetAllHandler>();
    services.AddScoped<IInstructionCreateHandler, InstructionCreateHandler>();

    services.AddScoped<IConfigUpdateHandler, ConfigUpdateHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agent")
      .WithTags("Agent");

    group.MapAgentAuthEndpoint();
    group.MapAgentReportCreateEndpoint();
    group.MapGetAllAgentsEndpoint();
    group.MapUpdateStatusEndpoint();
    group.MapGetByIdAgentEndpoint();

    group.MapGetAllInstructionsEndpoint();
    group.MapCreateInstructionEndpoint();

    group.MapConfigUpdateEndpoint();
  }
}

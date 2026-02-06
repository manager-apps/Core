using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Agent.Auth;
using Server.Api.Features.Agent.Config.Get;
using Server.Api.Features.Agent.Config.Update;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Hardware.Get;
using Server.Api.Features.Agent.Report;
using Server.Api.Features.Agent.State.Update;

namespace Server.Api.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentAuthHandler, AgentAuthHandler>();
    services.AddScoped<IAgentReportCreateHandler, AgentReportCreateHandler>();
    services.AddScoped<IAgentGetAllHandler, AgentGetAllHandler>();
    services.AddScoped<IStateUpdateHandler, StateUpdateHandler>();
    services.AddScoped<IAgentGetByIdHandler, AgentGetByIdHandler>();

    services.AddScoped<IInstructionsGetAllHandler, InstructionsGetAllHandler>();
    services.AddScoped<IInstructionCreateHandler, InstructionCreateHandler>();

    services.AddScoped<IConfigGetHandler, ConfigGetHandler>();
    services.AddScoped<IConfigUpdateHandler, ConfigUpdateHandler>();

    services.AddScoped<IHardwareGetHandler, HardwareGetHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agents");

    group.MapAgentAuthEndpoint();
    group.MapAgentReportCreateEndpoint();
    group.MapGetAllAgentsEndpoint();
    group.MapUpdateStateEndpoint();
    group.MapGetByIdAgentEndpoint();

    group.MapGetAllInstructionsEndpoint();
    group.MapCreateInstructionEndpoint();

    group.MapConfigGetEndpoint();
    group.MapConfigUpdateEndpoint();

    group.MapHardwareGetEndpoint();
  }
}

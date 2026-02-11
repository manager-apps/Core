using Server.Api.Features.Agent.Cert.Revoke;
using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Agent.Config.Get;
using Server.Api.Features.Agent.Config.Update;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Hardware.Get;

namespace Server.Api.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentGetAllHandler, AgentGetAllHandler>();
    services.AddScoped<IAgentGetByIdHandler, AgentGetByIdHandler>();
    services.AddScoped<IAgentInstructionsGetAllHandler, AgentInstructionsGetAllHandler>();
    services.AddScoped<IAgentInstructionCreateHandler, AgentInstructionCreateHandler>();
    services.AddScoped<IAgentConfigGetHandler, AgentConfigGetHandler>();
    services.AddScoped<IAgentConfigUpdateHandler, AgentConfigUpdateHandler>();
    services.AddScoped<IAgentHardwareGetHandler, AgentHardwareGetHandler>();
    services.AddScoped<IAgentCertRevokeHandler, AgentCertRevokeHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agents");

    group.MapGetAllAgentsEndpoint();
    group.MapGetByIdAgentEndpoint();
    group.MapGetAllInstructionsEndpoint();
    group.MapCreateInstructionEndpoint();
    group.MapConfigGetEndpoint();
    group.MapConfigUpdateEndpoint();
    group.MapHardwareGetEndpoint();
    group.MapRevokeCertificateEndpoint();
  }
}

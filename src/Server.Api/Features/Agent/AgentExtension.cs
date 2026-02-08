using Server.Api.Features.Agent.Cert.Create;
using Server.Api.Features.Agent.Cert.Revoke;
using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Agent.Config.Get;
using Server.Api.Features.Agent.Config.Update;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Hardware.Get;
using Server.Api.Features.Agent.State.Update;

namespace Server.Api.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentGetAllHandler, AgentGetAllHandler>();
    services.AddScoped<IStateUpdateHandler, StateUpdateHandler>();
    services.AddScoped<IAgentGetByIdHandler, AgentGetByIdHandler>();

    services.AddScoped<IInstructionsGetAllHandler, InstructionsGetAllHandler>();
    services.AddScoped<IInstructionCreateHandler, InstructionCreateHandler>();

    services.AddScoped<IConfigGetHandler, ConfigGetHandler>();
    services.AddScoped<IConfigUpdateHandler, ConfigUpdateHandler>();

    services.AddScoped<IHardwareGetHandler, HardwareGetHandler>();

    services.AddScoped<IEnrollmentTokenCreateHandler, EnrollmentTokenCreateHandler>();
    services.AddScoped<ICertRevokeHandler, CertRevokeHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agents");

    group.MapGetAllAgentsEndpoint();
    group.MapUpdateStateEndpoint();
    group.MapGetByIdAgentEndpoint();

    group.MapGetAllInstructionsEndpoint();
    group.MapCreateInstructionEndpoint();

    group.MapConfigGetEndpoint();
    group.MapConfigUpdateEndpoint();

    group.MapHardwareGetEndpoint();

    group.MapCreateEnrollmentTokenEndpoint();
    group.MapRevokeCertificateEndpoint();
  }
}

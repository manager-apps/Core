using WebApi.Features.Agent.Auth;
using WebApi.Features.Agent.Report;

namespace WebApi.Features.Agent;

public static class AgentExtension
{
  public static void AddAgentServices(this IServiceCollection services)
  {
    services.AddScoped<IAgentAuthHandler, AgentAuthHandler>();
    services.AddScoped<IAgentReportHandler, AgentReportHandler>();
  }

  public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/agent")
      .WithTags("Agent");

    group.MapAgentAuthEndpoint();
    group.MapAgentReportEndpoint();
  }
}

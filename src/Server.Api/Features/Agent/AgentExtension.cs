namespace Server.Api.Features.Agent;

public static class AgentExtension
{


  public static void MapAgentFeatures(this IEndpointRouteBuilder app)
  {
    app.MapAgentLoginEndpoint();
  }
}

using Microsoft.AspNetCore.Mvc;

namespace Server.Api.Features.Agent;

internal record AgentLoginRequest(
  string AgentId,
  string ClientSecretKey);

internal record AgentLoginResponse(
  string AuthToken,
  string RefreshToken);

internal static class AgentLoginFeature
{
  internal static void MapAgentLoginEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost($"/agent/api/v1/authenticate", (
      [FromBody] AgentLoginRequest request,
      CancellationToken cancellationToken) => Task.FromResult(new AgentLoginResponse(
        AuthToken: $"auth-token-for-{request.AgentId}",
        RefreshToken: $"refresh-token-for-{request.AgentId}")))
    .WithTags("Agent")
    .Produces<AgentLoginResponse>();
}


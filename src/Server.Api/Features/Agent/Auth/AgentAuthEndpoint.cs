using Common;
using Common.Messages;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.Auth;

internal static class AgentAuthEndpoint
{
  internal static void MapAgentAuthEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("auth",
        async (
            [FromBody] AuthMessageRequest request,
            [FromHeader(Name = Headers.Tag)] string tag,
            [FromHeader(Name = Headers.AgentVersion)] string agentVersion,
            [FromServices] IAgentAuthHandler handler,
            CancellationToken ct)
          => (await handler.AuthenticateAsync(request, tag, agentVersion, ct)).ToApiResult())
      .WithDescription(
      @"
          Authenticate an agent and issue a token. If the agent does not exist,
          a new agent record will be created, with Inactive status, and an administrator
          must activate the agent before it can be used. While the agent is inactive,
          authentication will fail.
       ")
      .WithTags("Agent")
      .Produces<AuthMessageResponse>()
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

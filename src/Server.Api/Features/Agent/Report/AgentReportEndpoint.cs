using Common.Messages;
using Server.Api.Common.Result;

namespace Server.Api.Features.Agent.Report;
using Microsoft.AspNetCore.Mvc;

internal static class AgentReportEndpoint
{
  internal static void MapAgentReportEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("report", async (
        [FromBody] ReportMessageRequest request,
        [FromServices] IAgentReportHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, cancellationToken)).ToApiResult())
      .RequireAuthorization()
      .WithDescription(
      @"
        This endpoint allows an agent to report the results of executed instructions
        and receive new instructions to execute. The agent sends a ReportMessageRequest
        containing the results of previously assigned instructions. The server processes
        these results, updates the instruction statuses accordingly, and then responds
        with a ReportMessageResponse containing new instructions for the agent to execute.
      ")
      .Produces<ReportMessageResponse>();
}

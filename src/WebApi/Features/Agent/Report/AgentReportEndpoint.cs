using Common.Messages;
using WebApi.Common.Result;

namespace WebApi.Features.Agent.Report;
using Microsoft.AspNetCore.Mvc;

internal static class AgentReportEndpoint
{
  internal static void MapAgentReportEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("/v1/report", async (
        [FromBody] ReportMessageRequest request,
        [FromServices] IAgentReportHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, cancellationToken)).ToApiResult())
      .RequireAuthorization()
      .WithTags("Agent")
      .WithName("Report")
      .WithSummary("Report instruction results and receive new instructions.")
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

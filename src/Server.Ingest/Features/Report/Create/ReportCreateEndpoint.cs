using Common.Messages;
using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Report.Create;

internal static class AgentReportCreateEndpoint
{
  internal static void MapAgentReportCreateEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("/api/v1/agents/report", async (
          [FromBody] ReportMessageRequest request,
          [FromServices] IReportCreateHandler handler,
          HttpContext context,
          CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, cancellationToken)).ToApiResult())
      .WithTags("Agent")
      .Produces<ReportMessageResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status401Unauthorized);
}

using Common.Messages;
using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Report.Create;

internal static class AgentReportCreateEndpoint
{
  internal static void MapAgentReportCreateEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("report", async (
          [FromBody] ReportMessageRequest request,
          [FromServices] IReportCreateHandler handler,
          HttpContext context,
          CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, cancellationToken)).ToApiResult())
      .RequireAuthorization()
      .Produces<ReportMessageResponse>()
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

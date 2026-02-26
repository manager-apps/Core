using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;
using Common.Messages;
using Common;
namespace Server.Ingest.Features.Sync;

internal static class SyncEndpoint
{
  internal static void MapSyncEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("sync", async (
          [FromBody] SyncMessageRequest request,
          [FromServices] ISyncHandler handler,
          [FromHeader(Name = Headers.AgentVersion)] string agentVersion,
          [FromHeader(Name = Headers.Tag)] string tag,
          HttpContext context,
          CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, tag, agentVersion, cancellationToken))
        .ToApiResult())
      .RequireAuthorization()
      .Produces<SyncMessageResponse>()
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

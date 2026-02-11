using Common.Messages;
using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Extensions;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Sync;

internal static class SyncEndpoint
{
  internal static void MapSyncEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("sync", async (
          [FromBody] SyncMessageRequest request,
          [FromServices] ISyncHandler handler,
          HttpContext context,
          CancellationToken cancellationToken)
        => (await handler.HandleAsync(context.User, request, cancellationToken))
        .ToApiResult())
      .RequireAuthorization()
      .Produces<SyncMessageResponse>()
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .MapToApiVersion(ApiVersioningExtension.V1);
}

using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;

namespace Server.Api.Features.Chat;

internal static class ChatEndpoint
{
    internal static void MapChatStreamEndpoint(this IEndpointRouteBuilder app)
      => app.MapPost("/stream", async (
          [FromBody] ChatRequest request,
          [FromServices] IChatHandler handler,
          HttpContext httpContext,
          CancellationToken cancellationToken) =>
        {
          httpContext.Response.ContentType = "text/event-stream";
          httpContext.Response.Headers["Cache-Control"] = "no-cache";
          httpContext.Response.Headers["Connection"] = "keep-alive";
          await foreach (var chunk in handler.StreamChatAsync(request, cancellationToken))
          {
              await httpContext.Response.WriteAsync(chunk, cancellationToken);
              await httpContext.Response.Body.FlushAsync(cancellationToken);
          }
        })
        .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .MapToApiVersion(ApiVersioningExtension.V1);
}

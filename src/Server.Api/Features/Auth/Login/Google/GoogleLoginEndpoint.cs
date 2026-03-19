using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;

namespace Server.Api.Features.Auth.Login.Google;

internal static class GoogleLoginEndpoint
{
  internal static void MapGoogleLoginEndpoint(this IEndpointRouteBuilder app)
    => app.MapPost("/login/google", async (
        [FromBody] GoogleLoginRequest request,
        [FromServices] IGoogleLoginHandler handler,
        CancellationToken ct) =>
      {
        var result = await handler.HandleAsync(request, ct);
        return result is null ? Results.Unauthorized() : Results.Ok(result);
      })
      .AllowAnonymous()
      .MapToApiVersion(ApiVersioningExtension.V1);
}

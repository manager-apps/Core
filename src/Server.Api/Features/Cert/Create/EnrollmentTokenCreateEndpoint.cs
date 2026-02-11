using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Extensions;
using Server.Api.Common.Result;

namespace Server.Api.Features.Cert.Create;

internal static class EnrollmentTokenCreateEndpoint
{
  internal static void MapCreateEnrollmentTokenEndpoint(this IEndpointRouteBuilder app)
  {
    app.MapPost("/tokens", async (
      [FromBody] CreateEnrollmentTokenRequest request,
      [FromServices] IEnrollmentTokenCreateHandler handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);
      return result.ToApiResult();
    })
    .Produces<EnrollmentTokenResponse>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .MapToApiVersion(ApiVersioningExtension.V1);
  }
}

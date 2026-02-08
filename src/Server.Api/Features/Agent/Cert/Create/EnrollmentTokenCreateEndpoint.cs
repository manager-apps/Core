using Microsoft.AspNetCore.Mvc;
using Server.Api.Common.Result;
using Server.Api.Features.Cert;

namespace Server.Api.Features.Agent.Cert.Create;

public static class EnrollmentTokenCreateEndpoint
{
  public static void MapCreateEnrollmentTokenEndpoint(this IEndpointRouteBuilder app)
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
    .ProducesProblem(StatusCodes.Status400BadRequest);
  }
}

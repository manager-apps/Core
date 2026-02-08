using Microsoft.AspNetCore.Mvc;
using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert.Revocation;

public static class CertRevocationEndpoint
{
    public static void MapCertRevocationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/agents/certificates/revocation/{thumbprint}", async (
            [FromRoute] string thumbprint,
            [FromServices] ICertRevocationHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.CheckRevocationAsync(thumbprint, ct);
            return result.ToApiResult();
        })
        .WithName("CheckCertificateRevocation")
        .WithSummary("Check if a certificate is revoked")
        .WithDescription("Returns the revocation status of a certificate by thumbprint.")
        .Produces<CertRevocationResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .AllowAnonymous(); // Allow anonymous access - agents need to check revocation before authenticating
    }
}

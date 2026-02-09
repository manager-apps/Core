using Server.Api.Common.Result;

namespace Server.Api.Features.Cert;

internal static class CertificateErrors
{
  internal static Error CertificateNotFound() =>
    Error.NotFound("Certificate not found.");
}

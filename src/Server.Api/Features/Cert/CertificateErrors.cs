using Server.Api.Common.Result;

namespace Server.Api.Features.Cert;

public static class CertificateErrors
{
  public static Error CertificateNotFound() =>
    Error.NotFound("Certificate not found.");
}

using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Cert;

/// <summary>
/// Certificate-related domain errors.
/// </summary>
public static class CertErrors
{
    public static Error AgentNotFound() =>
       Error.NotFound("Agent not found for the provided credentials.");

    public static Error InvalidCredentials()
      => Error.Unauthorized("Invalid credentials provided for certificate enrollment.");

    public static Error InvalidEnrollmentToken()
      => Error.Unauthorized("Invalid enrollment token provided for certificate enrollment.");

    public static Error CertificateNotFound()
      => Error.NotFound("No certificate found for the agent. Enrollment is required.");

    public static Error RenewalNotAllowed()
      => Error.Forbidden("Certificate renewal is not allowed. Please enroll a new certificate.");

    public static Error EnrollmentFailed(string reason)
      => Error.Internal($"Certificate enrollment failed: {reason}");
}

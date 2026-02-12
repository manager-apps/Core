using Server.Ingest.Common.Result;

namespace Server.Ingest.Features.Report;

public static class ReportErrors
{
  public static Error AgentUnauthorized() =>
    Error.Unauthorized("Agent is not authorized to report data.");

  public static Error AgentNotFound() =>
    Error.NotFound("Agent not found.");
}

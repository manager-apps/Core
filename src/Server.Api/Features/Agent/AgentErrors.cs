using Server.Api.Common.Result;

namespace Server.Api.Features.Agent;

public static class AgentErrors
{
  public static Error NotFound()
    => Error.NotFound("Agent not found.");
}

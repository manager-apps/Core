using Server.Api.Common.Result;

namespace Server.Api.Features.Agent;

public class AgentErrors
{
  public static Error NotFound()
    => Error.NotFound("Agent not found.");
}

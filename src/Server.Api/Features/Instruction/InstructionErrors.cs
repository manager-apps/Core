using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction;

public class InstructionErrors
{
  public static Error AgentNotFound(long agentId)
    => Error.NotFound($"Agent with ID {agentId} not found.");
}

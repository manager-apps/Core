using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction;

public class InstructionErrors
{
  public static Error NotFound()
    => Error.NotFound("Instruction not found.");
}

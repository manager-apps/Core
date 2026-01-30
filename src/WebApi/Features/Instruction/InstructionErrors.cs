using WebApi.Common.Result;

namespace WebApi.Features.Instruction;

public class InstructionErrors
{
  public static Error NotFound()
    => Error.NotFound("Instruction not found.");
}

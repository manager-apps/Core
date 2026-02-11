using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction;

internal static class InstructionErrors
{
  internal static Error NotFound()
    => Error.NotFound($"Instruction was not found.");
}

using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction;

internal static class InstructionErrors
{
  internal static Error NotFound(long instructionId)
    => Error.NotFound($"Instruction with ID {instructionId} was not found.");
}

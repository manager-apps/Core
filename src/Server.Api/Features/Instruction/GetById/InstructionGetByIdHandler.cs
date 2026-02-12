using Server.Api.Common.Result;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Instruction.GetById;

internal interface IInstructionGetByIdHandler
{
  /// <summary>
  /// Gets an instruction by its ID.
  /// </summary>
  Task<Result<InstructionResponse>> HandleAsync(
    long instructionId,
    CancellationToken cancellationToken);
}

internal class InstructionGetByIdHandler(
  AppDbContext dbContext
) : IInstructionGetByIdHandler {
  public async Task<Result<InstructionResponse>> HandleAsync(
    long instructionId,
    CancellationToken cancellationToken)
  {
    var instruction = await dbContext.Instructions
      .FindAsync([instructionId], cancellationToken);

    if(instruction is null)
      return InstructionErrors.NotFound();

    return instruction.ToResponse();
  }
}

using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;
using Server.Api.Common.Result;

namespace Server.Api.Features.Instruction.GetAll;

internal interface IGetAllInstructionsHandler
{
  /// <summary>
  /// Handles the retrieval of all instructions.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<IEnumerable<InstructionResponse>> HandleAsync(
    CancellationToken cancellationToken);
}

internal class GetAllInstructionsHandler(
  ILogger<GetAllInstructionsHandler> logger,
  AppDbContext dbContext
) : IGetAllInstructionsHandler
{
  public async Task<IEnumerable<InstructionResponse>> HandleAsync(
    CancellationToken cancellationToken)
  {
    var instructions = await dbContext.Instructions
      .AsNoTracking()
      .ToListAsync(cancellationToken);

    logger.LogInformation("Retrieved {InstructionCount} instructions", instructions.Count);
    return instructions.Select(i => i.ToResponse());
  }
}

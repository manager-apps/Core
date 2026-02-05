using Microsoft.EntityFrameworkCore;
using Server.Api.Features.Instruction;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Instruction.GetAll;

internal interface IInstructionsGetAllHandler
{
  /// <summary>
  /// Handles the retrieval of all instructions for a given agent
  /// </summary>
  Task<IEnumerable<InstructionResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class InstructionsGetAllHandler(
  ILogger<InstructionsGetAllHandler> logger,
  AppDbContext context
) : IInstructionsGetAllHandler {
  public async Task<IEnumerable<InstructionResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    var instructions = await context.Instructions
      .AsNoTracking()
      .Where(i => i.AgentId == agentId)
      .ToListAsync(cancellationToken);

    logger.LogInformation("Retrieved {InstructionCount} instructions for agent {AgentId}",
      instructions.Count, agentId);

    return instructions.Select(i => i.ToResponse());
  }
}

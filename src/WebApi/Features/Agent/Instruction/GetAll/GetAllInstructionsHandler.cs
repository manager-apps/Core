using Microsoft.EntityFrameworkCore;
using WebApi.Features.Instruction;
using WebApi.Infrastructure;

namespace WebApi.Features.Agent.Instruction.GetAll;

internal interface IGetAllInstructionsHandler
{
  Task<IEnumerable<InstructionResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

public class GetAllInstructionsHandler(
  ILogger<GetAllInstructionsHandler> logger,
  AppDbContext context
) : IGetAllInstructionsHandler
{
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

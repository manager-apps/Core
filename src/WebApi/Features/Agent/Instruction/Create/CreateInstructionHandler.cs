using WebApi.Common.Result;
using WebApi.Features.Instruction;
using WebApi.Infrastructure;

namespace WebApi.Features.Agent.Instruction.Create;

public interface ICreateInstructionHandler
{
  /// <summary>
  /// Creates an instruction for the specified agent.
  /// </summary>
  /// <param name="agentId"></param>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<InstructionResponse>> HandleAsync(
    long agentId,
    CreateAgentInstructionRequest request,
    CancellationToken cancellationToken);
}

public class CreateInstructionHandler (
  ILogger<CreateInstructionHandler> logger,
  AppDbContext dbContext
) : ICreateInstructionHandler
{
  public async Task<Result<InstructionResponse>> HandleAsync(
    long agentId,
    CreateAgentInstructionRequest request,
    CancellationToken cancellationToken)
  {
    var instruction = request.ToDomain(agentId);

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created instruction {InstructionId} for agent {AgentId}",
      instruction.Id,
      agentId);

    return instruction.ToResponse();
  }
}

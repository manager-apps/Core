using WebApi.Common.Result;
using WebApi.Infrastructure;

namespace WebApi.Features.Instruction.Create;

internal interface ICreateInstructionHandler
{
  /// <summary>
  /// Create a new instruction
  /// </summary>
  /// <param name="request"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<InstructionResponse>> HandleAsync(
    CreateInstructionRequest request,
    CancellationToken cancellationToken);
}

internal class CreateInstructionHandler(
  ILogger<CreateInstructionHandler> logger,
  AppDbContext dbContext
): ICreateInstructionHandler
{
  public async Task<Result<InstructionResponse>> HandleAsync(
    CreateInstructionRequest request,
    CancellationToken cancellationToken)
  {
    var domain = request.ToDomain();

    dbContext.Instructions.Add(domain);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created instruction {InstructionId} for agent {AgentId}",
      domain.Id, domain.AgentId);

    return domain.ToResponse();
  }
}

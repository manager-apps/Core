using Server.Api.Common.Result;
using Server.Api.Features.Instruction;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Instruction.Create;

internal interface IAgentInstructionCreateHandler
{
  /// <summary>
  /// Creates a shell command instruction for the specified agent.
  /// </summary>
  Task<Result<InstructionResponse>> HandleShellCommandAsync(
    long agentId,
    CreateShellCommandRequest request,
    CancellationToken cancellationToken);

  /// <summary>
  /// Creates a GPO set instruction for the specified agent.
  /// </summary>
  Task<Result<InstructionResponse>> HandleGpoSetAsync(
    long agentId,
    CreateGpoSetRequest request,
    CancellationToken cancellationToken);
}

internal class AgentInstructionCreateHandler (
  ILogger<AgentInstructionCreateHandler> logger,
  AppDbContext dbContext
) : IAgentInstructionCreateHandler {
  public async Task<Result<InstructionResponse>> HandleShellCommandAsync(
    long agentId,
    CreateShellCommandRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation(
      "Creating shell command instruction for agent {AgentId} with command: {Command}",
      agentId,
      request.Command);

    var instruction = request.ToDomain(agentId);

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created shell command instruction {InstructionId} for agent {AgentId}",
      instruction.Id,
      agentId);

    return instruction.ToResponse();
  }

  public async Task<Result<InstructionResponse>> HandleGpoSetAsync(
    long agentId,
    CreateGpoSetRequest request,
    CancellationToken cancellationToken)
  {
    logger.LogInformation(
      "Creating GPO set instruction for agent {AgentId} with GPO",
      agentId);

    var instruction = request.ToDomain(agentId);

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created GPO set instruction {InstructionId} for agent {AgentId}",
      instruction.Id,
      agentId);

    return instruction.ToResponse();
  }
}

using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Agent;
using Server.Api.Features.Instruction;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Instruction.Create;

public interface ICreateInstructionHandler
{
  /// <summary>
  /// Creates an instruction for the specified agent.
  /// </summary>
  Task<Result<InstructionResponse>> HandleAsync(
    long agentId,
    CreateAgentInstructionRequest request,
    CancellationToken cancellationToken);

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

  /// <summary>
  /// Creates a config sync instruction for the specified agent.
  /// </summary>
  Task<Result<InstructionResponse>> HandleConfigSyncAsync(
    long agentId,
    CreateConfigSyncRequest request,
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

  public async Task<Result<InstructionResponse>> HandleShellCommandAsync(
    long agentId,
    CreateShellCommandRequest request,
    CancellationToken cancellationToken)
  {
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
    var instruction = request.ToDomain(agentId);

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created GPO set instruction {InstructionId} for agent {AgentId}",
      instruction.Id,
      agentId);

    return instruction.ToResponse();
  }

  public async Task<Result<InstructionResponse>> HandleConfigSyncAsync(
    long agentId,
    CreateConfigSyncRequest request,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .Include(a => a.Config)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

    if (agent is null)
      return InstructionErrors.AgentNotFound(agentId);

    var currentConfig = agent.Config.ToMessage();
    var instruction = request.ToDomain(agentId, currentConfig);

    dbContext.Instructions.Add(instruction);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Created config sync instruction {InstructionId} for agent {AgentId}",
      instruction.Id,
      agentId);

    return instruction.ToResponse();
  }
}

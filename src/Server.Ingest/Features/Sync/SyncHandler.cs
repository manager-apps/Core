using System.Security.Claims;
using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Ingest.Common.Result;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Features.Sync;

/// <summary>
/// Handler for agent synchronization.
/// </summary>
public interface ISyncHandler
{
  /// <summary>
  /// Handles the synchronization request from an agent. Updates the agent's hardware info and last seen timestamp,
  /// </summary>
  Task<Result<SyncMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    SyncMessageRequest request,
    CancellationToken cancellationToken);
}

internal sealed class SyncHandler(
  ILogger<SyncHandler> logger,
  AppDbContext dbContext) : ISyncHandler
{
  public async Task<Result<SyncMessageResponse>> HandleAsync(
    ClaimsPrincipal agent,
    SyncMessageRequest request,
    CancellationToken cancellationToken)
  {
    var agentName = agent.Identity?.Name;
    if (string.IsNullOrEmpty(agentName))
      return Error.Unauthorized("Agent identity is missing or invalid.");

    var agentEntity = await dbContext.Agents
      .AsSplitQuery()
      .Include(a => a.Hardware)
      .Include(a => a.Config)
      .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
    if (agentEntity is null)
      return Error.NotFound("Agent not found.");

    if (agentEntity.Hardware is null)
    {
      logger.LogInformation("Creating hardware for agent: {AgentName}", agentName);
      var hardware = Hardware.Create(
        request.Hardware.OsVersion,
        request.Hardware.MachineName,
        request.Hardware.ProcessorCount,
        request.Hardware.TotalMemoryBytes);
      agentEntity.AssignHardware(hardware);
    }
    else
    {
      agentEntity.Hardware.Update(
        request.Hardware.OsVersion,
        request.Hardware.MachineName,
        request.Hardware.ProcessorCount,
        request.Hardware.TotalMemoryBytes);
    }

    if (agentEntity.Config is null)
    {
      logger.LogWarning("Creating default config for agent: {AgentName}", agentName);
      var config = Config.Create(
        request.Config.IterationDelaySeconds,
        request.Config.AuthenticationExitIntervalSeconds,
        request.Config.RunningExitIntervalSeconds,
        request.Config.ExecutionExitIntervalSeconds,
        request.Config.InstructionsExecutionLimit,
        request.Config.InstructionResultsSendLimit,
        request.Config.MetricsSendLimit,
        request.Config.AllowedCollectors,
        request.Config.AllowedInstructions);
      agentEntity.AssignConfig(config);
    }
    else
    {
      agentEntity.Config.Update(
        request.Config.IterationDelaySeconds,
        request.Config.AuthenticationExitIntervalSeconds,
        request.Config.RunningExitIntervalSeconds,
        request.Config.ExecutionExitIntervalSeconds,
        request.Config.InstructionsExecutionLimit,
        request.Config.InstructionResultsSendLimit,
        request.Config.MetricsSendLimit,
        request.Config.AllowedCollectors,
        request.Config.AllowedInstructions);
    }

    agentEntity.UpdateLastSeen(
      currentTag: agentEntity.CurrentTag,
      version: "1.0.0");

    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation(
      "Agent {AgentName} synchronized. Hardware: {Machine}, Processors: {ProcessorCount}",
      agentName,
      request.Hardware.MachineName,
      request.Hardware.ProcessorCount);

    if (!agentEntity.TryGetConfig(out var agentConfig) || agentConfig is null)
    {
      logger.LogError("Config is missing for agent {AgentName} after sync", agentName);
      return Error.Internal("Configuration is missing after synchronization.");
    }

    return new SyncMessageResponse(
      Config: new ConfigMessage(
        agentConfig.AuthenticationExitIntervalSeconds,
        agentConfig.RunningExitIntervalSeconds,
        agentConfig.ExecutionExitIntervalSeconds,
        agentConfig.InstructionsExecutionLimit,
        agentConfig.InstructionResultsSendLimit,
        agentConfig.MetricsSendLimit,
        agentConfig.IterationDelaySeconds,
        agentConfig.GetAllowedCollectorsList(),
        agentConfig.GetAllowedInstructionsList()));
  }
}

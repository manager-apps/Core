using Common.Messages;
using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Interfaces;
using Server.Api.Common.Result;
using Server.Api.Infrastructure;
using Server.Domain;

namespace Server.Api.Features.Agent.Auth;

internal interface IAgentAuthHandler
{
  /// <summary>
  /// Authenticate agent
  /// </summary>
  /// <param name="request"></param>
  /// <param name="currentTag">Agent currentTag from X-Tag header</param>
  /// <param name="currentVersion">Agent currentVersion from X-Version header</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<Result<AuthMessageResponse>> AuthenticateAsync(
    AuthMessageRequest request,
    string currentTag,
    string currentVersion,
    CancellationToken cancellationToken);
}

internal class AgentAuthHandler(
  ILogger<AgentAuthHandler> logger,
  IDataHasher dataHasher,
  IJwtProvider jwtProvider,
  AppDbContext dbContext) : IAgentAuthHandler
{
  public async Task<Result<AuthMessageResponse>> AuthenticateAsync(
    AuthMessageRequest request,
    string currentTag,
    string currentVersion,
    CancellationToken cancellationToken)
  {
    logger.LogInformation("Authenticating agent: {AgentName}", request.AgentName);

    var existingAgent = await FindAgentAsync(request.AgentName, cancellationToken);
    return existingAgent is not null
      ? await AuthenticateExistingAgentAsync(
          existingAgent,
          request,
          currentTag,
          currentVersion,
          cancellationToken)
      : await RegisterNewAgentAsync(
          request,
          currentTag,
          currentVersion,
          cancellationToken);
  }

  #region Private methods

  private async Task<Server.Domain.Agent?> FindAgentAsync(
    string agentName,
    CancellationToken cancellationToken)
  {
    return await dbContext.Agents
      .Include(a => a.Config)
      .Include(a => a.Hardware)
      .FirstOrDefaultAsync(a => a.Name == agentName, cancellationToken);
  }

  private async Task<Result<AuthMessageResponse>> AuthenticateExistingAgentAsync(
    Server.Domain.Agent agent,
    AuthMessageRequest request,
    string currentTag,
    string currentVersion,
    CancellationToken cancellationToken)
  {
    if (!agent.CanAuthenticate())
    {
      logger.LogWarning("Authentication attempt for inactive agent: {AgentName}", agent.Name);
      return AgentErrors.Unauthorized();
    }

    if (!dataHasher.IsDataValid(request.SecretKey, agent.SecretKeyHash, agent.SecretKeySalt))
      return AgentErrors.Unauthorized();

    agent.UpdateLastSeen(currentTag);
    agent.Hardware.Update(
      osVersion: request.Hardware.OsVersion,
      machineName: request.Hardware.MachineName,
      processorCount: request.Hardware.ProcessorCount,
      totalMemoryBytes: request.Hardware.TotalMemoryBytes);

    await dbContext.SaveChangesAsync(cancellationToken);

    var token = jwtProvider.GenerateTokenForAgent(agent.Name);
    logger.LogInformation("Authenticated existing agent: {AgentName}", agent.Name);

    var configResponse = ReturnIfChag(agent.Config, request.Config);
    return new AuthMessageResponse(
      AuthToken: token,
      RefreshToken: "refresh-token-placeholder",
      Config: configResponse);
  }

  private async Task<Result<AuthMessageResponse>> RegisterNewAgentAsync(
    AuthMessageRequest request,
    string currentTag,
    string currentVersion,
    CancellationToken cancellationToken)
  {
    var (hash, salt) = dataHasher.CreateDataHash(request.SecretKey);
    var agent = request.ToDomain(hash, salt, currentTag, currentVersion);

    dbContext.Agents.Add(agent);
    await dbContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Registered new agent: {AgentName}", agent.Name);

    // New agent is inactive by default - cannot authenticate yet,
    // Note: Admin should activate the agent from the client to allow
    // it to authenticate and start communicating with the server
    return AgentErrors.Unauthorized();
  }


  private static ConfigMessage? ReturnIfChag(
    Config? config,
    ConfigMessage requestConfig)
  {
    if (config is null)
      return null;

    var serverCollectors = config.GetAllowedCollectorsList();
    var serverInstructions = config.GetAllowedInstructionsList();

    var isEqual =
      config.RunningExitIntervalSeconds == requestConfig.RunningExitIntervalSeconds
      && config.ExecutionExitIntervalSeconds == requestConfig.ExecutionExitIntervalSeconds
      && config.AuthenticationExitIntervalSeconds == requestConfig.AuthenticationExitIntervalSeconds
      && config.SynchronizationExitIntervalSeconds == requestConfig.SynchronizationExitIntervalSeconds
      && config.InstructionsExecutionLimit == requestConfig.InstructionsExecutionLimit
      && config.InstructionResultsSendLimit == requestConfig.InstructionResultsSendLimit
      && config.MetricsSendLimit == requestConfig.MetricsSendLimit
      && serverCollectors.SequenceEqual(requestConfig.AllowedCollectors)
      && serverInstructions.SequenceEqual(requestConfig.AllowedInstructions);

    if (isEqual)
      return null;

    // Return server config to override agent's custom configuration
    return new ConfigMessage(
      AuthenticationExitIntervalSeconds: config.AuthenticationExitIntervalSeconds,
      SynchronizationExitIntervalSeconds: config.SynchronizationExitIntervalSeconds,
      RunningExitIntervalSeconds: config.RunningExitIntervalSeconds,
      ExecutionExitIntervalSeconds: config.ExecutionExitIntervalSeconds,
      InstructionsExecutionLimit: config.InstructionsExecutionLimit,
      InstructionResultsSendLimit: config.InstructionResultsSendLimit,
      MetricsSendLimit: config.MetricsSendLimit,
      AllowedCollectors: serverCollectors,
      AllowedInstructions: serverInstructions);
  }

  #endregion
}

using Server.Api.Features.Config;
using Server.Api.Features.Hardware;
using Server.Domain;

namespace Server.Api.Features.Agent;

/// <summary>
/// Represents the response model for an agent, containing its details and state information.
/// </summary>
public record AgentResponse(
  long Id,
  string Name,
  string CurrentTag,
  string SourceTag,
  string Version,
  AgentState State,
  DateTimeOffset CreatedAt,
  DateTimeOffset LastUpdatedAt,
  DateTimeOffset? UpdatedAt);

/// <summary>
/// Represents the detailed response model for an agent, including config and hardware.
/// </summary>
public record AgentDetailResponse(
  long Id,
  string Name,
  string CurrentTag,
  string SourceTag,
  string Version,
  AgentState State,
  DateTimeOffset CreatedAt,
  DateTimeOffset LastUpdatedAt,
  DateTimeOffset? UpdatedAt,
  ConfigResponse Config,
  HardwareResponse Hardware);

/// <summary>
/// Request model for updating an agent's state.
/// </summary>
/// <param name="NewState"></param>
public record AgentUpdateStateRequest(
  AgentState NewState);


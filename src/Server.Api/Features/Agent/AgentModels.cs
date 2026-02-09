using Server.Api.Features.Config;
using Server.Api.Features.Hardware;

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
  DateTimeOffset CreatedAt,
  DateTimeOffset LastUpdatedAt,
  DateTimeOffset? UpdatedAt,
  ConfigResponse? Config,
  HardwareResponse? Hardware);


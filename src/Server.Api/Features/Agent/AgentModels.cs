using Server.Api.Features.Config;
using Server.Api.Features.Hardware;

namespace Server.Api.Features.Agent;

public record AgentResponse(
  long Id,
  string Name,
  string CurrentTag,
  string SourceTag,
  string Version,
  DateTimeOffset CreatedAt,
  DateTimeOffset LastUpdatedAt,
  DateTimeOffset? UpdatedAt);

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


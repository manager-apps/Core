using Server.Domain;

namespace Server.Api.Features.Agent;

public record AgentResponse(
  long Id,
  string Name,
  AgentState State,
  DateTimeOffset CreatedAt,
  DateTimeOffset LastUpdatedAt,
  DateTimeOffset? UpdatedAt);

public record AgentUpdateStateRequest(
  AgentState NewState);


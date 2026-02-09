using Server.Api.Features.Config;
using Server.Api.Features.Hardware;
using Common.Messages;

namespace Server.Api.Features.Agent;

public static class AgentMapper
{
  extension(Server.Domain.Agent agent)
  {
    public AgentResponse ToResponse()
      => new(
          Id: agent.Id,
          Name: agent.Name,
          SourceTag: agent.SourceTag,
          CurrentTag: agent.CurrentTag,
          Version: agent.Version,
          CreatedAt: agent.CreatedAt,
          LastUpdatedAt: agent.LastSeenAt,
          UpdatedAt: agent.UpdatedAt);

    public AgentDetailResponse ToDetailResponse()
      => new(
          Id: agent.Id,
          Name: agent.Name,
          SourceTag: agent.SourceTag,
          CurrentTag: agent.CurrentTag,
          Version: agent.Version,
          CreatedAt: agent.CreatedAt,
          LastUpdatedAt: agent.LastSeenAt,
          UpdatedAt: agent.UpdatedAt,
          Config: agent.Config?.ToResponse(),
          Hardware: agent.Hardware?.ToResponse());
  }
}

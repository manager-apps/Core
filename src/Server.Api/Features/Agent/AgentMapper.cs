using Server.Api.Features.Config;
using Server.Api.Features.Hardware;
using Common.Messages;

namespace Server.Api.Features.Agent;

public static class AgentMapper
{
  extension(AuthMessageRequest request)
  {
    public Server.Domain.Agent ToDomain(
      byte[] secretKeyHash,
      byte[] secretKeySalt,
      string tag,
      string version)
      => Domain.Agent.Create(
          config: request.Config.ToDomain(),
          hardware: request.Hardware.ToDomain(),
          name: request.AgentName,
          sourceTag: tag,
          secretKeyHash: secretKeyHash,
          secretKeySalt: secretKeySalt);
  }

  extension(Server.Domain.Agent agent)
  {
    public AgentResponse ToResponse()
      => new(
          Id: agent.Id,
          Name: agent.Name,
          SourceTag: agent.SourceTag,
          CurrentTag: agent.CurrentTag,
          State: agent.State,
          CreatedAt: agent.CreatedAt,
          LastUpdatedAt: agent.LastSeenAt,
          UpdatedAt: agent.UpdatedAt);

    public AgentDetailResponse ToDetailResponse()
      => new(
          Id: agent.Id,
          Name: agent.Name,
          SourceTag: agent.SourceTag,
          CurrentTag: agent.CurrentTag,
          State: agent.State,
          CreatedAt: agent.CreatedAt,
          LastUpdatedAt: agent.LastSeenAt,
          UpdatedAt: agent.UpdatedAt,
          Config: agent.Config.ToResponse(),
          Hardware: agent.Hardware.ToResponse());
  }
}

using Common.Messages;

namespace Server.Api.Features.Agent;

public static class AgentMapper
{
  extension(AuthMessageRequest request)
  {
    public Server.Domain.Agent ToDomain(
      byte[] secretKeyHash,
      byte[] secretKeySalt)
      => Domain.Agent.Create(
        name: request.AgentName,
        tag: request.Tag,
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
  }
}

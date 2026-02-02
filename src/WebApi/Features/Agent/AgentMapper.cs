using Common.Messages;

namespace WebApi.Features.Agent;

public static class AgentMapper
{
  extension(AuthMessageRequest request)
  {
    public Server.Domain.Agent ToDomain(
      byte[] secretKeyHash,
      byte[] secretKeySalt)
      => Server.Domain.Agent.Create(
        name: request.AgentName,
        secretKeyHash: secretKeyHash,
        secretKeySalt: secretKeySalt);
  }

  extension(Server.Domain.Agent agent)
  {
    public AgentResponse ToResponse()
      => new(
        Id: agent.Id,
        Name: agent.Name,
        State: agent.State,
        CreatedAt: agent.CreatedAt,
        LastUpdatedAt: agent.LastSeenAt,
        UpdatedAt: agent.UpdatedAt);
  }
}

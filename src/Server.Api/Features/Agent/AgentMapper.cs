using Common.Messages;

namespace Server.Api.Features.Agent;

public static class AgentMapper
{
  extension(LoginMessageRequest request)
  {
    public Domain.Agent ToDomain(
      byte[] secretKeyHash,
      byte[] secretKeySalt)
      => Domain.Agent.Create(
        name: request.AgentName,
        secretKeyHash: secretKeyHash,
        secretKeySalt: secretKeySalt);
  }

  extension(Domain.Agent agent)
  {
  }
}

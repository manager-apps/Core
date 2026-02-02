namespace Server.Api.Common.Interfaces;

public interface IJwtProvider
{
  /// <summary>
  /// Generates a JWT token for a user with the specified username and role.
  /// </summary>
  string GenerateTokenForUser(string username, string role);

  /// <summary>
  /// Generates a JWT token for an agent with the specified agent name.
  /// </summary>
  string GenerateTokenForAgent(string agentName);
}

namespace Common.Messages;

public record AuthMessageResponse(
  string AuthToken,
  string RefreshToken);

public record AuthMessageRequest(
  string AgentName,
  string SecretKey,
  string AreaName);

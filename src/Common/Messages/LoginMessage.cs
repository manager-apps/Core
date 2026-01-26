namespace Common.Messages;

public record LoginMessageResponse(
  string AuthToken,
  string RefreshToken);

public record LoginMessageRequest(
  string AgentName,
  string SecretKey);

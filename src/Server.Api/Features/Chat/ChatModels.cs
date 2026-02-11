namespace Server.Api.Features.Chat;

/// <summary>
/// Request to send a chat message to the AI assistant
/// </summary>
public record ChatRequest(
    string Message,
    string? ConversationId = null);

/// <summary>
/// Response from the AI assistant
/// </summary>
public record ChatResponse(
    string Message,
    string ConversationId);

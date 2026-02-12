namespace Server.Api.Common.Interfaces;

/// <summary>
/// Represents a tool/function that the AI can call.
/// </summary>
public record ChatServiceTool(
  string Name,
  string Description,
  string ParametersJson,
  Func<string, CancellationToken, Task<string>> Handler);

public abstract record ChatServiceResponse
{
  /// <summary>
  /// A text chunk response from the AI.
  /// </summary>
  public record TextChunk(string Text) : ChatServiceResponse;

  /// <summary>
  /// Indicates that the conversation has ended.
  /// </summary>
  public record ConversationEnd : ChatServiceResponse;
}

public interface IChatService
{
  /// <summary>
  /// Streams a chat completion response from the AI model.
  /// </summary>
  IAsyncEnumerable<ChatServiceResponse> StreamChatCompletionAsync(
    string systemPrompt,
    string userMessage,
    IEnumerable<ChatServiceTool> tools,
    CancellationToken cancellationToken);
}

using System.Runtime.CompilerServices;
using Server.Api.Common.Interfaces;
using OpenAI.Chat;

namespace Server.Api.Infrastructure;

public class OpenAiChatService(
  ILogger<OpenAiChatService> logger,
  ChatClient client) : IChatService
{
  public async IAsyncEnumerable<ChatServiceResponse> StreamChatCompletionAsync(
    string systemPrompt,
    string userMessage,
    IEnumerable<ChatServiceTool> tools,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var toolsList = tools.ToList();

    var chatOptions = new ChatCompletionOptions();
    foreach (var tool in toolsList)
    {
      var chatTool = ChatTool.CreateFunctionTool(
        functionName: tool.Name,
        functionDescription: tool.Description,
        functionParameters: BinaryData.FromString(tool.ParametersJson)
      );

      chatOptions.Tools.Add(chatTool);
    }

    var messages = new List<ChatMessage>
    {
      ChatMessage.CreateSystemMessage(systemPrompt),
      ChatMessage.CreateUserMessage(userMessage)
    };

    var toolHandlerMap = toolsList.ToDictionary(t => t.Name, t => t.Handler);
    while (!cancellationToken.IsCancellationRequested)
    {
      var completion = await client.CompleteChatAsync(
        messages,
        chatOptions,
        cancellationToken);

      if (completion.Value.Content.Count > 0 &&
          !string.IsNullOrEmpty(completion.Value.Content[0].Text))
      {
        var text = completion.Value.Content[0].Text;
        messages.Add(ChatMessage.CreateAssistantMessage(text));

        foreach (var c in text)
        {
          yield return new ChatServiceResponse.TextChunk(c.ToString());
          await Task.Delay(10, cancellationToken);
        }
      }

      if (completion.Value.FinishReason != ChatFinishReason.ToolCalls ||
          completion.Value.ToolCalls.Count == 0)
      {
        yield return new ChatServiceResponse.ConversationEnd();
        yield break;
      }

      messages.Add(new AssistantChatMessage(completion.Value));

      foreach (var toolCall in completion.Value.ToolCalls)
      {
        logger.LogInformation(
          "AI requested tool call: {ToolName} with args: {Args}",
          toolCall.FunctionName,
          toolCall.FunctionArguments);

        if (!toolHandlerMap.TryGetValue(toolCall.FunctionName, out var handler))
        {
          var errorMsg = $"Unknown tool: {toolCall.FunctionName}";
          logger.LogWarning(errorMsg);
          messages.Add(new ToolChatMessage(toolCall.Id, errorMsg));
          continue;
        }

        string result;
        try
        {
          result = await handler(
            toolCall.FunctionArguments.ToString(),
            cancellationToken);

          logger.LogInformation(
            "Tool {ToolName} executed successfully",
            toolCall.FunctionName);
        }
        catch (Exception ex)
        {
          logger.LogError(
            ex,
            "Error executing tool {ToolName}",
            toolCall.FunctionName);

          result = System.Text.Json.JsonSerializer.Serialize(new
          {
            success = false,
            error = $"Tool execution error: {ex.Message}"
          });
        }
        messages.Add(new ToolChatMessage(toolCall.Id, result));
      }
    }
  }
}

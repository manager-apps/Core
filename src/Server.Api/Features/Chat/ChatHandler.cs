using System.Runtime.CompilerServices;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.Instruction.Create;
using Server.Api.Features.Instruction;
using Server.Api.Features.Instruction.GetById;
using System.Text.Json;
using OpenAI.Chat;
using Server.Api.Features.Agent.GetById;
using Server.Api.Features.Agent.Instruction.GetAll;

namespace Server.Api.Features.Chat;

internal interface IChatHandler
{
  /// <summary>
  /// Streams a chat response from the AI assistant in real-time as
  /// it's generated. This is useful for providing faster feedback
  /// to the user and improving perceived responsiveness.
  /// </summary>
  IAsyncEnumerable<string> StreamChatAsync(
    ChatRequest request,
    CancellationToken cancellationToken);
}

internal class ChatHandler : IChatHandler
{
  private record Tool (
    string Description,
    BinaryData Parameters,
    Func<string, CancellationToken, Task<string>> Handler);

  private string SystemPrompt =>
  $"""
    You are an AI assistant for a Device Control and Information (DCI) management system.
    You help users manage and monitor Windows agents that report metrics and execute instructions.

    Available tools:
    {string.Join("\n", _toolHandlers.Select(t => $"- {t.Key}: {t.Value.Description}"))}

    **Important Instructions:**
    - Be proactive: if user asks to do something that requires agent interaction, don't ask for confirmation,
      just create the instruction
    - Always confirm what you're doing and show the result
    - When the instruction created say it will be executed shortly, but don't wait for the result to come back to continue the conversation.
      The user might ask to do other things while waiting for the instruction results.
    - return the id of the created instruction so user can check its status later if they want
    - if user ask about give me instruction with id X, give them the details of the instruction with id X, including its status, type, parameters, output, and error messages if there is any
    - if user ask what is wrong with instruction with id X, but the instruction with id X doesn't have error message, just say "Instruction with id X doesn't have error message, but I will update you if I get any error information about it"]
    - if user ask what is wrong with instruction with id X, and instruction X has error message, show the error message and say "Instruction with id X has error message: [error message], but I will update you if I get any new information about it"]
    - when user ask why did the instruction with id X fail, give them very detailed answer based on the error message of instruction X, and if the error message is not detailed enough, say "The error message for instruction with id X is not detailed enough to determine the exact reason for failure, but based on the information I have, it might be because [your best guess based on the error message]. I will update you if I get any new information about it"
   """;

  private readonly ILogger<ChatHandler> _logger;
  private readonly Dictionary<string, Tool> _toolHandlers;
  private readonly ChatClient _client;

  public ChatHandler(
    ILogger<ChatHandler> logger,
    IAgentGetAllHandler getAllAgentsHandler,
    IAgentInstructionsGetAllHandler getAgentInstructionsHandler,
    IInstructionGetByIdHandler getInstructionByIdHandler,
    ChatClient client
  ) {
    _logger = logger;
    _client = client;

    _toolHandlers = new Dictionary<string, Tool>
    {
      ["GetAgentInstructions"] = new(
        "Get a list of all instructions for a specific agent, including their status, types, parameters, and results.",
        Parameters: BinaryData.FromString(
        """
          {
            "type": "object",
            "additionalProperties": false,
            "properties": {
              "agentId": {
                "type": "integer",
                "description": "The ID of the agent to retrieve instructions for"
              }
            },
            "required": ["agentId"]
          }
        """),
        async (args, ct) =>
        {
          _logger.LogInformation("GetAgentInstructions called with args: '{Args}'", args);
          if (string.IsNullOrWhiteSpace(args))
          {
            _logger.LogWarning("GetAgentInstructions received empty arguments");
            return JsonSerializer.Serialize(new
            {
              success = false,
              error = "No arguments provided. Please specify agentId."
            });
          }
          try
          {
            var json = JsonDocument.Parse(args);
            var agentId = json.RootElement.GetProperty("agentId").GetInt64();

            var instructions = await getAgentInstructionsHandler.HandleAsync(agentId, ct);
            return JsonSerializer.Serialize(new
            {
              success = true,
              instructions
            });
          }
          catch (JsonException ex)
          {
            _logger.LogError(ex, "Failed to parse GetAgentInstructions arguments: '{Args}'", args);
            return JsonSerializer.Serialize(new
            {
              success = false,
              error = $"Invalid JSON arguments: {ex.Message}"
            });
          }
        }),

      ["GetInstructionById"] = new(
        "Get detailed information about a specific instruction by its ID, including status, type, parameters, output, and error messages.",
        Parameters: BinaryData.FromString(
        """
          {
            "type": "object",
            "additionalProperties": false,
            "properties": {
              "instructionId": {
                "type": "integer",
                "description": "The ID of the instruction to retrieve"
              }
            },
            "required": ["instructionId"]
          }
        """),
        async (args, ct) =>
        {
          _logger.LogInformation("GetInstructionById called with args: '{Args}'", args);
          if (string.IsNullOrWhiteSpace(args))
          {
            _logger.LogWarning("GetInstructionById received empty arguments");
            return JsonSerializer.Serialize(new
            {
              success = false,
              error = "No arguments provided. Please specify instructionId."
            });
          }
          try
          {
            var json = JsonDocument.Parse(args);
            var instructionId = json.RootElement.GetProperty("instructionId").GetInt64();

            var result = await getInstructionByIdHandler.HandleAsync(instructionId, ct);

            if (!result.IsSuccess)
            {
              return JsonSerializer.Serialize(new
              {
                success = false,
                error = result.Error
              });
            }

            return JsonSerializer.Serialize(new
            {
              success = true,
              instruction = result.Value
            });
          }
          catch (JsonException ex)
          {
            _logger.LogError(ex, "Failed to parse GetInstructionById arguments: '{Args}'", args);
            return JsonSerializer.Serialize(new
            {
              success = false,
              error = $"Invalid JSON arguments: {ex.Message}"
            });
          }
        }),

      ["GetAllAgents"] = new(
        "Get a list of all agents in the system with their current status, tags, versions, and last seen times.",
        Parameters: BinaryData.FromString(
        """
          {
            "type": "object",
            "additionalProperties": false,
            "properties": {},
            "required": []
          }
        """),
        async (_, ct) =>
        {
          var agents = await getAllAgentsHandler.HandleAsync(ct);
          return JsonSerializer.Serialize(agents);
        })
    };
  }

  public async IAsyncEnumerable<string> StreamChatAsync(
    ChatRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var tools = _toolHandlers.Select(kvp =>
      ChatTool.CreateFunctionTool(
        functionName: kvp.Key,
        functionDescription: kvp.Value.Description,
        functionParameters: kvp.Value.Parameters
      )
    );

    var chatOptions = new ChatCompletionOptions();
    foreach (var tool in tools)
      chatOptions.Tools.Add(tool);

    var messages = new List<ChatMessage>
    {
      ChatMessage.CreateSystemMessage(SystemPrompt),
      ChatMessage.CreateUserMessage(request.Message)
    };

    while (!cancellationToken.IsCancellationRequested)
    {
      var completion = await _client.CompleteChatAsync(messages, chatOptions, cancellationToken);

      if (completion.Value.Content.Count > 0 &&
          !string.IsNullOrEmpty(completion.Value.Content[0].Text))
      {
        var text = completion.Value.Content[0].Text;
        foreach (var c in text)
        {
          yield return c.ToString();
          await Task.Delay(10, cancellationToken);
        }

        messages.Add(ChatMessage.CreateAssistantMessage(text));
      }

      if (completion.Value.FinishReason != ChatFinishReason.ToolCalls ||
          completion.Value.ToolCalls.Count == 0)
        yield break;

      messages.Add(new AssistantChatMessage(completion.Value));
      foreach (var toolCall in completion.Value.ToolCalls)
      {
        if (!_toolHandlers.TryGetValue(toolCall.FunctionName, out var tool))
        {
          messages.Add(new ToolChatMessage(
            toolCall.Id,
            $"Unknown tool: {toolCall.FunctionArguments}"));

          continue;
        }

        string result;
        try
        {
          result = await tool.Handler(
            toolCall.FunctionArguments.ToString(),
            cancellationToken);
        }
        catch (Exception ex)
        {
          result = JsonSerializer.Serialize(new
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

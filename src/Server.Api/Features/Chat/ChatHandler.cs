using System.Runtime.CompilerServices;
using System.Text.Json;
using Server.Api.Common.Interfaces;
using Server.Api.Features.Agent.GetAll;
using Server.Api.Features.Agent.Instruction.GetAll;
using Server.Api.Features.Instruction.GetById;

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

internal class ChatHandler(
  ILogger<ChatHandler> logger,
  IChatService chatService,
  IAgentGetAllHandler getAllAgentsHandler,
  IAgentInstructionsGetAllHandler getAgentInstructionsHandler,
  IInstructionGetByIdHandler getInstructionByIdHandler)
  : IChatHandler
{
  private static string SystemPrompt =>
  $"""
    You are an AI assistant for a Device Control and Information (DCI) management system.
    You help users manage and monitor Windows agents that report metrics and execute instructions.
    Available tools:
    - GetAgentInstructions: Get a list of all instructions for a specific agent
    - GetInstructionById: Get detailed information about a specific instruction by its ID
    - GetAllAgents: Get a list of all agents in the system
   """;

  public async IAsyncEnumerable<string> StreamChatAsync(
    ChatRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    await foreach (var response in chatService.StreamChatCompletionAsync(
      SystemPrompt,
      request.Message,
      GetAvailableTools(),
      cancellationToken))
    {
      switch (response)
      {
        case ChatServiceResponse.TextChunk textChunk:
          yield return textChunk.Text;
          break;
        case ChatServiceResponse.ConversationEnd:
          yield break;
      }
    }
  }

  private IEnumerable<ChatServiceTool> GetAvailableTools() =>
  [
    new(
      Name: "GetAgentInstructions",
      Description: "Get a list of all instructions for a specific agent, including their status, types, parameters, and results.",
      ParametersJson:
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
      """,
      Handler: async (args, ct) =>
      {
        logger.LogInformation("GetAgentInstructions called with args: '{Args}'", args);
        if (string.IsNullOrWhiteSpace(args))
        {
          logger.LogWarning("GetAgentInstructions received empty arguments");
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
          logger.LogError(ex, "Failed to parse GetAgentInstructions arguments: '{Args}'", args);
          return JsonSerializer.Serialize(new
          {
            success = false,
            error = $"Invalid JSON arguments: {ex.Message}"
          });
        }
      }),

    new(
      Name: "GetInstructionById",
      Description: "Get detailed information about a specific instruction by its ID, including status, type, parameters, output, and error messages.",
      ParametersJson:
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
      """,
      Handler: async (args, ct) =>
      {
        logger.LogInformation("GetInstructionById called with args: '{Args}'", args);
        if (string.IsNullOrWhiteSpace(args))
        {
          logger.LogWarning("GetInstructionById received empty arguments");
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
          logger.LogError(ex, "Failed to parse GetInstructionById arguments: '{Args}'", args);
          return JsonSerializer.Serialize(new
          {
            success = false,
            error = $"Invalid JSON arguments: {ex.Message}"
          });
        }
      }),

    new(
      Name: "GetAllAgents",
      Description: "Get a list of all agents in the system with their current status, tags, versions, and last seen times.",
      ParametersJson:
      """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {},
          "required": []
        }
      """,
      Handler: async (_, ct) =>
      {
        var agents = await getAllAgentsHandler.HandleAsync(ct);
        return JsonSerializer.Serialize(new
        {
          success = true,
          agents
        });
      })
  ];
}

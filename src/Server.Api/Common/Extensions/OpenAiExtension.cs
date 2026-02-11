using OpenAI.Chat;
using Server.Api.Common.Interfaces;
using Server.Api.Infrastructure;

namespace Server.Api.Common.Extensions;

public static class OpenAiExtension
{
  public static void AddOpenAi(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<ChatClient>(_ =>
    {
      var apiKey = configuration.GetSection("Ai:OpenAi:ApiKey").Get<string>()!;
      var model = configuration.GetSection("Ai:OpenAi:Model").Get<string>()!;

      return new ChatClient(model, apiKey);
    });

    services.AddScoped<IChatService, OpenAiChatService>();
  }
}

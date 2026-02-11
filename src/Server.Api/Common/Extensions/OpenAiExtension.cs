using OpenAI.Chat;

namespace Server.Api.Common.Extensions;

public static class OpenAiExtension
{
  extension(IServiceCollection services)
  {
    public void AddOpenAi(IConfiguration configuration)
    {
      services.AddSingleton<ChatClient>(_ =>
      {
        var apiKey = configuration.GetSection("Ai:OpenAi:ApiKey").Get<string>()!;
        var model = configuration.GetSection("Ai:OpenAi:Model").Get<string>()!;

        return new ChatClient(model, apiKey);
      });
    }
  }
}

using Server.Api.Common.Extensions;

namespace Server.Api.Features.Chat;

internal static class ChatExtension
{
  internal static void AddChatServices(this IServiceCollection services)
  {
    services.AddScoped<IChatHandler, ChatHandler>();
  }

  internal static void MapChatEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app
      .MapGroup("/chat");

    group.MapChatStreamEndpoint();
  }
}

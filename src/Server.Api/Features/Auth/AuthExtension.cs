using Server.Api.Features.Auth.Login.Google;

namespace Server.Api.Features.Auth;

public static class AuthExtension
{
  public static void AddAuthServices(this IServiceCollection services)
  {
    services.AddScoped<IGoogleLoginHandler, GoogleLoginHandler>();
  }

  public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/auth");
    group.MapGoogleLoginEndpoint();
  }
}

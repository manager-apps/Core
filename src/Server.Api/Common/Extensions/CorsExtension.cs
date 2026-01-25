namespace Server.Api.Common.Extensions;

public static class CorsExtension
{
  public static void AddCors(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddCors(options =>
    {
      options.AddDefaultPolicy(policy =>
      {
        // todo: replace from configuration
        policy.WithOrigins("http://localhost:3000")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
      });
    });
  }
}

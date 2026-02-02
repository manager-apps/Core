namespace Server.Api.Common.Extensions;

public static class CorsExtension
{
  extension(IServiceCollection services)
  {
    public void AddCors(IConfiguration configuration)
     => services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
        {
          policy.WithOrigins(configuration
              .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }));
  }
}


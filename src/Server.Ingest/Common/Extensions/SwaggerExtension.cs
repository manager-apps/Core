using Microsoft.OpenApi;

namespace Server.Ingest.Common.Extensions;

public static class SwaggerExtension
{
  extension(IServiceCollection services)
  {
    public void AddSwagger(IConfiguration configuration)
    {
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen(options =>
      {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
          Title = "Ingest API",
          Version = "v1",
          Description = "API for ingesting data from agents"
        });
      });
    }
  }

  extension(WebApplication app)
  {
    public void UseSwaggerDocs()
    {
      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Manager API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Manager API V2");
        options.RoutePrefix = string.Empty;
      });
    }
  }
}

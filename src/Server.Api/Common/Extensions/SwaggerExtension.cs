namespace Server.Api.Common.Extensions;

public static class SwaggerExtension
{
  extension(IServiceCollection services)
  {
    public void AddSwagger(IConfiguration configuration)
    {
      services.AddEndpointsApiExplorer();
      services.AddSwaggerGen();
    }
  }

  extension(WebApplication app)
  {
    public void UseSwaggerDocs()
    {
      app.UseSwagger();
      app.UseSwagger();
      app.UseSwaggerUI(options =>
      {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
      });
    }
  }
}

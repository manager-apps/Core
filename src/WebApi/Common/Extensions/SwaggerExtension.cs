namespace WebApi.Common.Extensions;

public static class SwaggerExtension
{
  public static void AddSwagger(this IServiceCollection services)
  {
    services.AddOpenApi();
    services.AddSwaggerGen();
  }

  public static void UseSwaggerUI(this WebApplication app)
  {
    if (!app.Environment.IsDevelopment())
      return;

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
      options.RoutePrefix = string.Empty;
    });
  }
}

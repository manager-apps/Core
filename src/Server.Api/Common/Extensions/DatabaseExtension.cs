using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;

namespace Server.Api.Common.Extensions;

public static class DatabaseExtension
{
  public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration["Database:Postgres:ConnectionString"]!;
    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(connectionString));
  }

  public static async Task ApplyMigrationsAsync(this WebApplication app)
  {
    var autoMigrate = app.Configuration.GetValue<bool>("Database:Postgres:AutoMigrate");
    if (!autoMigrate)
      return;

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();
  }
}

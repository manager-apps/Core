using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure;

namespace WebApi.Common.Extensions;

public static class DatabaseExtension
{
  extension(IServiceCollection services)
  {
    public void AddPsqlDatabase(IConfiguration configuration)
    {
      var connectionString = configuration["Database:Postgres:ConnectionString"]!;
      services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    }

    public void AddClickHouseDatabase(IConfiguration configuration)
    {
      var connectionString = configuration["Database:ClickHouse:ConnectionString"]!;
      services.AddSingleton<ClickHouse.Driver.ADO.ClickHouseConnection>(_ =>
        new ClickHouse.Driver.ADO.ClickHouseConnection(connectionString));
      services.AddScoped<Interfaces.IMetricStorage, ClickHouseMetricStorage>();
    }
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

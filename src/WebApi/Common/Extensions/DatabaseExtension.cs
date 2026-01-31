using System.Reflection;
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
    if (autoMigrate)
    {
      await using var scope = app.Services.CreateAsyncScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await dbContext.Database.MigrateAsync();
    }

    var autoMigrateClickHouse = app.Configuration.GetValue<bool>("Database:ClickHouse:AutoMigrate");
    if (autoMigrateClickHouse)
    {
      await ApplyClickHouseMigrationsAsync(app);
    }
  }

  private static async Task ApplyClickHouseMigrationsAsync(WebApplication app)
  {
    try
    {
      var connection = app.Services.GetRequiredService<ClickHouse.Driver.ADO.ClickHouseConnection>();
      var assembly = Assembly.GetExecutingAssembly();
      var migrationFolder = "WebApi.Migrations.Clickhouse";

      var resourceNames = assembly.GetManifestResourceNames()
        .Where(r => r.StartsWith(migrationFolder) && r.EndsWith(".sql"))
        .OrderBy(r => r)
        .ToList();

      if (!resourceNames.Any())
      {
        app.Logger.LogWarning("No ClickHouse migration files found.");
        return;
      }

      foreach (var resourceName in resourceNames)
      {
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var sql = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(sql))
          continue;

        try
        {
          await connection.OpenAsync();
          using var command = connection.CreateCommand();
          command.CommandText = sql;
          await command.ExecuteNonQueryAsync();
          connection.Close();

          app.Logger.LogInformation("Applied ClickHouse migration: {MigrationName}",
            resourceName.Split('.').LastOrDefault());
        }
        catch (Exception ex)
        {
          app.Logger.LogError(ex, "Failed to apply ClickHouse migration: {MigrationName}",
            resourceName.Split('.').LastOrDefault());
          throw;
        }
      }

      app.Logger.LogInformation("ClickHouse migrations applied successfully.");
    }
    catch (Exception ex)
    {
      app.Logger.LogError(ex, "Error applying ClickHouse migrations.");
      throw;
    }
  }
}

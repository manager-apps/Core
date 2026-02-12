using Microsoft.EntityFrameworkCore;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Common.Extensions;

internal static class DatabaseExtension
{
  extension(IServiceCollection services)
  {
    internal void AddPsqlDatabase(IConfiguration configuration)
    {
      var connectionString = configuration["Database:Postgres:ConnectionString"]!;
      services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    }
  }
}

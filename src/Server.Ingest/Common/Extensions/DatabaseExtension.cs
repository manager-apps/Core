using Microsoft.EntityFrameworkCore;
using Server.Ingest.Infrastructure;

namespace Server.Ingest.Common.Extensions;

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
  }
}

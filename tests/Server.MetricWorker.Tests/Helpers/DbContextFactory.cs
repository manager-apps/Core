using Microsoft.EntityFrameworkCore;
using Server.MetricWorker.Infrastructure;

namespace Server.MetricWorker.Tests.Helpers;

internal static class DbContextFactory
{
  public static AppDbContext Create()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }
}

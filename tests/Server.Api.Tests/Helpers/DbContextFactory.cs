using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;

namespace Server.Api.Tests.Helpers;

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

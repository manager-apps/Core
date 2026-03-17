using Microsoft.EntityFrameworkCore;
using Server.InstructionWorker.Infrastructure;

namespace Server.InstructionWorker.Tests.Helpers;

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

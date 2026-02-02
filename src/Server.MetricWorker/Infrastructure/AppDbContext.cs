using Microsoft.EntityFrameworkCore;
using Server.Domain;

namespace Server.MetricWorker.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
  public DbSet<Agent> Agents => Set<Agent>();
}

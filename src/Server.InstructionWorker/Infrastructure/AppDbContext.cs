using Microsoft.EntityFrameworkCore;
using Server.Domain;

namespace Server.InstructionWorker.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
  public DbSet<Instruction> Instructions => Set<Instruction>();
}

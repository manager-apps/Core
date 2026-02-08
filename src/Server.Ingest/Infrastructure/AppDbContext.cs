using Microsoft.EntityFrameworkCore;
using Server.Domain;

namespace Server.Ingest.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Instruction> Instructions => Set<Instruction>();
  public DbSet<Agent> Agents => Set<Agent>();
  public DbSet<Hardware> Hardwares => Set<Hardware>();
  public DbSet<Config> Configs => Set<Config>();
  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
  public DbSet<Certificate> Certificates => Set<Certificate>();
  public DbSet<EnrollmentToken> EnrollmentTokens => Set<EnrollmentToken>();
}

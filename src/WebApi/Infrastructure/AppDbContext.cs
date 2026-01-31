using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Domain.Instruction> Instructions => Set<Domain.Instruction>();
  public DbSet<Domain.Agent> Agents => Set<Domain.Agent>();
}

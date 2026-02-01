using Microsoft.EntityFrameworkCore;
using WebApi.Domain;
using WebApi.Infrastructure;

namespace WebApi.Common.Extensions;

public static class LoadTestSeeder
{
  private const string TestSecretKey = "test-secret-key";

  /// <summary>
  /// Seeds the database with test data for load testing.
  /// </summary>
  private static async Task SeedAsync(AppDbContext db, int agents = 1000, int instructionsPerAgent = 200)
  {
    if (!await db.Agents.AnyAsync())
    {
      var agentsToAdd = new List<Agent>();
      var dataHasher = new HmacDataHasher();
      var (hash, salt) = dataHasher.CreateDataHash(TestSecretKey);

      for (int i = 1; i <= agents; i++)
      {
        var agent = Agent.Create(
          name: $"agent-{i:D6}",
          secretKeyHash: hash,
          secretKeySalt: salt);
        agent.MarkAsActive();
        agentsToAdd.Add(agent);
      }

      db.Agents.AddRange(agentsToAdd);
      await db.SaveChangesAsync();
    }

    if (await db.Instructions.AnyAsync()) return;

    var agentIds = await db.Agents
      .OrderBy(a => a.Id)
      .Take(agents)
      .Select(a => a.Id)
      .ToListAsync();
    var allInstructions = new List<Instruction>();
    foreach (var agentId in agentIds)
    {
      var batch = Enumerable.Range(0, instructionsPerAgent)
        .Select(_ => Instruction.Create(
          agentId,
          InstructionType.ShellCommand,
          """{"$type":"shell","command":"echo test","timeout":5000}"""));
      allInstructions.AddRange(batch);
    }
    const int batchSize = 10000;
    for (int i = 0; i < allInstructions.Count; i += batchSize)
    {
      var batch = allInstructions.Skip(i).Take(batchSize);
      db.Instructions.AddRange(batch);
      await db.SaveChangesAsync();
    }
  }

  /// <summary>
  /// Extension method to easily seed load test data from the application startup.
  /// </summary>
  public static async Task SeedLoadTestDataAsync(
    this WebApplication app,
    int agents = 1000,
    int instructionsPerAgent = 200)
  {
    var seedLoadTestData = app.Configuration.GetValue("SeedLoadTestData", false);

    if (!seedLoadTestData) return;

    app.Logger.LogInformation("Starting load test data seeding with {AgentCount} agents and {InstructionCount} instructions per agent...", agents, instructionsPerAgent);
    app.Logger.LogInformation("All test agents will use secret key: '{SecretKey}' and will be marked as ACTIVE", TestSecretKey);

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await SeedAsync(dbContext, agents, instructionsPerAgent);

    app.Logger.LogInformation("Load test data seeding completed successfully. All agents can now authenticate with the test secret key.");
  }
}

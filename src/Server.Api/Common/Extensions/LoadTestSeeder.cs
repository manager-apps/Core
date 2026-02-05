using Microsoft.EntityFrameworkCore;
using Server.Api.Infrastructure;
using Server.Domain;

namespace Server.Api.Common.Extensions;

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
      var random = new Random(42); // Fixed seed for reproducibility

      string[] osVersions = ["Windows 10 Pro 22H2", "Windows 11 Pro 23H2", "Windows Server 2022", "Windows Server 2019", "Ubuntu 22.04 LTS"];
      string[] collectors = ["cpu", "memory", "disk", "network", "process"];
      string[] instructions = ["shell", "file", "registry", "service"];

      for (int i = 1; i <= agents; i++)
      {
        var config = Config.Create(
          authenticationExitIntervalSeconds: random.Next(5, 30),
          synchronizationExitIntervalSeconds: random.Next(5, 30),
          runningExitIntervalSeconds: random.Next(5, 30),
          executionExitIntervalSeconds: random.Next(1, 10),
          instructionsExecutionLimit: random.Next(5, 50),
          instructionResultsSendLimit: random.Next(10, 100),
          metricsSendLimit: random.Next(50, 500),
          allowedCollectors: collectors.OrderBy(_ => random.Next()).Take(random.Next(1, collectors.Length + 1)).ToList(),
          allowedInstructions: instructions.OrderBy(_ => random.Next()).Take(random.Next(1, instructions.Length + 1)).ToList());

        var hardware = Hardware.Create(
          osVersion: osVersions[random.Next(osVersions.Length)],
          machineName: $"MACHINE-{i:D6}",
          processorCount: random.Next(2, 32),
          totalMemoryBytes: (long)random.Next(4, 128) * 1024 * 1024 * 1024);

        var agent = Agent.Create(
          config: config,
          hardware: hardware,
          version: "1.0.0",
          name: $"agent-{i:D6}",
          sourceTag: "load-test",
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
    app.Logger.LogInformation("Starting load test data seeding with {AgentCount} agents and {InstructionCount} instructions per agent...", agents, instructionsPerAgent);
    app.Logger.LogInformation("All test agents will use secret key: '{SecretKey}' and will be marked as ACTIVE", TestSecretKey);

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await SeedAsync(dbContext, agents, instructionsPerAgent);

    app.Logger.LogInformation("Load test data seeding completed successfully. All agents can now authenticate with the test secret key.");
  }
}

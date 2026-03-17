using Server.Api.Features.Agent;
using Server.Api.Features.Config;
using Server.Api.Features.Hardware;
using Server.Api.Features.Instruction;
using Server.Domain;
using Xunit;

namespace Server.Api.Tests.Features;

public class MapperTests
{
  // ── AgentMapper ──────────────────────────────────────────────────────────

  [Fact]
  public void Agent_ToResponse_MapsAllFields()
  {
    var agent = Agent.Create("MyAgent", "v1.0");

    var response = agent.ToResponse();

    Assert.Equal(agent.Id, response.Id);
    Assert.Equal("MyAgent", response.Name);
    Assert.Equal("v1.0", response.SourceTag);
    Assert.Equal("v1.0", response.CurrentTag);
  }

  [Fact]
  public void Agent_ToDetailResponse_WithNullConfigAndHardware()
  {
    var agent = Agent.Create("Agent1", "tag1");

    var response = agent.ToDetailResponse();

    Assert.Equal("Agent1", response.Name);
    Assert.Null(response.Config);
    Assert.Null(response.Hardware);
  }

  // ── HardwareMapper ────────────────────────────────────────────────────────

  [Fact]
  public void Hardware_ToResponse_MapsAllFields()
  {
    var hardware = Hardware.Create("Windows 11", "PC01", 8, 16_000_000_000L);

    var response = hardware.ToResponse();

    Assert.Equal("Windows 11", response.OsVersion);
    Assert.Equal("PC01", response.MachineName);
    Assert.Equal(8, response.ProcessorCount);
    Assert.Equal(16_000_000_000L, response.TotalMemoryBytes);
  }

  // ── ConfigMapper ──────────────────────────────────────────────────────────

  [Fact]
  public void Config_ToResponse_MapsAllFields()
  {
    var config = Config.Create(30, 300, 360, 120, 10, 20, 50, ["CPU", "RAM"], ["Shell"]);

    var response = config.ToResponse();

    Assert.Equal(30, response.IterationDelaySeconds);
    Assert.Equal(300, response.AuthenticationExitIntervalSeconds);
    Assert.Equal(360, response.RunningExitIntervalSeconds);
    Assert.Equal(120, response.ExecutionExitIntervalSeconds);
    Assert.Equal(10, response.InstructionsExecutionLimit);
    Assert.Equal(20, response.InstructionResultsSendLimit);
    Assert.Equal(50, response.MetricsSendLimit);
    Assert.Contains("CPU", response.AllowedCollectors);
    Assert.Contains("RAM", response.AllowedCollectors);
    Assert.Contains("Shell", response.AllowedInstructions);
  }

  [Fact]
  public void Config_ToResponse_EmptyCollectorsAndInstructions()
  {
    var config = Config.Create(30, 300, 360, 120, 10, 20, 50, [], []);

    var response = config.ToResponse();

    Assert.Empty(response.AllowedCollectors);
    Assert.Empty(response.AllowedInstructions);
  }

  // ── InstructionMapper ─────────────────────────────────────────────────────

  [Fact]
  public void Instruction_ToResponse_MapsAllFields()
  {
    var instruction = Instruction.Create(5L, InstructionType.Shell, "{\"cmd\":\"echo\"}");

    var response = instruction.ToResponse();

    Assert.Equal(5L, response.AgentId);
    Assert.Equal(InstructionType.Shell, response.Type);
    Assert.Equal(InstructionState.Pending, response.State);
    Assert.Equal("{\"cmd\":\"echo\"}", response.PayloadJson);
    Assert.Null(response.Output);
    Assert.Null(response.Error);
  }

  [Fact]
  public void CreateShellCommandRequest_ToDomain_CreatesShellInstruction()
  {
    var request = new CreateShellCommandRequest("dir", 3000);

    var instruction = request.ToDomain(agentId: 7L);

    Assert.Equal(7L, instruction.AgentId);
    Assert.Equal(InstructionType.Shell, instruction.Type);
    Assert.Equal(InstructionState.Pending, instruction.State);
    Assert.Contains("dir", instruction.PayloadJson);
  }

  [Fact]
  public void CreateGpoSetRequest_ToDomain_CreatesGpoInstruction()
  {
    var request = new CreateGpoSetRequest("DisableUSB", "Enabled");

    var instruction = request.ToDomain(agentId: 3L);

    Assert.Equal(3L, instruction.AgentId);
    Assert.Equal(InstructionType.Gpo, instruction.Type);
    Assert.Equal(InstructionState.Pending, instruction.State);
    Assert.Contains("DisableUSB", instruction.PayloadJson);
    Assert.Contains("Enabled", instruction.PayloadJson);
  }
}

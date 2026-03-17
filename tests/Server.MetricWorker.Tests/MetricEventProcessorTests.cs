using System.Text.Json;
using Common;
using Common.Events;
using Common.Messages;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Server.Domain;
using Server.MetricWorker.Interfaces;
using Server.MetricWorker.Tests.Helpers;
using Xunit;

namespace Server.MetricWorker.Tests;

public class MetricEventProcessorTests
{
  private static MetricEventProcessor CreateProcessor(HybridCache cache)
    => new(cache, NullLogger<MetricEventProcessor>.Instance);

  private static OutboxMessage CreateMessage(string payloadJson)
    => OutboxMessage.Create(payloadJson, nameof(AgentMetricsEvent));

  private static string SerializeEvent(string agentName, MetricMessage metric)
  {
    var evt = new AgentMetricsEvent(agentName, metric);
    return JsonSerializer.Serialize(evt, JsonOptions.Default);
  }

  private static MetricMessage CreateMetric()
    => new("cpu", "cpu_usage", 55.0, "%", DateTime.UtcNow, null);

  private static HybridCache CreateCacheMock(Agent? agent)
  {
    var cache = Substitute.For<HybridCache>();
    cache.GetOrCreateAsync<Agent?>(
        Arg.Any<string>(),
        Arg.Any<Func<CancellationToken, ValueTask<Agent?>>>(),
        Arg.Any<HybridCacheEntryOptions?>(),
        Arg.Any<IEnumerable<string>?>(),
        Arg.Any<CancellationToken>())
      .Returns(ValueTask.FromResult(agent));
    return cache;
  }

  [Fact]
  public async Task ProcessAsync_ReturnsFalse_WhenPayloadJsonIsInvalid()
  {
    using var db = DbContextFactory.Create();
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage("{{invalid-json}}");

    var result = await CreateProcessor(CreateCacheMock(null))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageFailed_WhenPayloadJsonIsInvalid()
  {
    using var db = DbContextFactory.Create();
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage("{{invalid-json}}");

    await CreateProcessor(CreateCacheMock(null))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Failed, message.State);
    Assert.Equal(1, message.RetryCount);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsFalse_WhenAgentNotFound()
  {
    using var db = DbContextFactory.Create();
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    var result = await CreateProcessor(CreateCacheMock(null))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.False(result);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageFailed_WhenAgentNotFound()
  {
    using var db = DbContextFactory.Create();
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    await CreateProcessor(CreateCacheMock(null))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Failed, message.State);
  }

  [Fact]
  public async Task ProcessAsync_DoesNotCallStorage_WhenAgentNotFound()
  {
    using var db = DbContextFactory.Create();
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    await CreateProcessor(CreateCacheMock(null))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    await storage.DidNotReceive().StoreAsync(
      Arg.Any<string>(), Arg.Any<MetricMessage>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ProcessAsync_CallsStorage_WithCorrectData_WhenAgentFound()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    var storage = Substitute.For<IMetricStorage>();
    var metric = CreateMetric();
    var message = CreateMessage(SerializeEvent("PC-001", metric));

    await CreateProcessor(CreateCacheMock(agent))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    await storage.Received(1).StoreAsync(
      "PC-001", Arg.Is<MetricMessage>(m => m.Type == metric.Type && m.Value == metric.Value),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageProcessed_WhenSuccessful()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    await CreateProcessor(CreateCacheMock(agent))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Processed, message.State);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsTrue_WhenSuccessful()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    var storage = Substitute.For<IMetricStorage>();
    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    var result = await CreateProcessor(CreateCacheMock(agent))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.True(result);
  }

  [Fact]
  public async Task ProcessAsync_MarksMessageFailed_WhenStorageThrows()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    var storage = Substitute.For<IMetricStorage>();
    storage.StoreAsync(Arg.Any<string>(), Arg.Any<MetricMessage>(), Arg.Any<CancellationToken>())
      .ThrowsAsync(new InvalidOperationException("ClickHouse unavailable"));

    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    await CreateProcessor(CreateCacheMock(agent))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.Equal(OutboxMessageState.Failed, message.State);
    Assert.Contains("ClickHouse unavailable", message.Error);
  }

  [Fact]
  public async Task ProcessAsync_ReturnsFalse_WhenStorageThrows()
  {
    using var db = DbContextFactory.Create();
    var agent = Agent.Create("PC-001", "tag1");
    var storage = Substitute.For<IMetricStorage>();
    storage.StoreAsync(Arg.Any<string>(), Arg.Any<MetricMessage>(), Arg.Any<CancellationToken>())
      .ThrowsAsync(new InvalidOperationException("ClickHouse unavailable"));

    var message = CreateMessage(SerializeEvent("PC-001", CreateMetric()));

    var result = await CreateProcessor(CreateCacheMock(agent))
      .ProcessAsync(message, db, storage, CancellationToken.None);

    Assert.False(result);
  }
}

using Common.Messages;

namespace Common.Events;

/// <summary>
/// Wrapper that includes agent context with metrics for outbox processing
/// </summary>
public record AgentMetricsEvent(
  string AgentName,
  MetricMessage Metric);

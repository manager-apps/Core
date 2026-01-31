using System.Net.NetworkInformation;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  /// <summary>
  /// Collect total TCP connections count
  /// </summary>
  private Domain.Metric CollectTotalTcpConnections()
  {
    try
    {
      var tcpConnections = IPGlobalProperties.GetIPGlobalProperties()
        .GetTcpIPv4Statistics().ConnectionsInitiated +
        IPGlobalProperties.GetIPGlobalProperties()
        .GetTcpIPv6Statistics().ConnectionsInitiated;

      var metric = new Domain.Metric
      {
        Type = "tcp_total_connections",
        Name = MetricConfig.Tcp.TotalConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = tcpConnections,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_total_connections",
        Name = MetricConfig.Tcp.TotalConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect TCP connections by state
  /// </summary>
  private IEnumerable<Domain.Metric> CollectTcpConnectionsByState()
  {
    var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
    var tcpConnections = ipProperties.GetActiveTcpConnections();

    var connectionStates = tcpConnections
      .GroupBy(x => x.State)
      .ToList();

    foreach (var stateGroup in connectionStates)
    {
      var stateName = stateGroup.Key.ToString();
      var count = stateGroup.Count();

      var metric = new Domain.Metric
      {
        Type = "tcp_connections_by_state",
        Name = stateName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = count,
        Metadata = new Dictionary<string, object>
        {
          { "state", stateName }
        }
      };

      yield return metric;
    }
  }

  /// <summary>
  /// Collect established TCP connections
  /// </summary>
  private Domain.Metric CollectEstablishedTcpConnections()
  {
    try
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      var establishedCount = ipProperties.GetActiveTcpConnections()
        .Count(x => x.State == TcpState.Established);

      var metric = new Domain.Metric
      {
        Type = "tcp_established_connections",
        Name = MetricConfig.Tcp.EstablishedConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = establishedCount,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_established_connections",
        Name = MetricConfig.Tcp.EstablishedConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect TCP connections in TIME_WAIT state
  /// </summary>
  private Domain.Metric CollectTimeWaitConnections()
  {
    try
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      var timeWaitCount = ipProperties.GetActiveTcpConnections()
        .Count(x => x.State == TcpState.TimeWait);

      var metric = new Domain.Metric
      {
        Type = "tcp_time_wait_connections",
        Name = MetricConfig.Tcp.TimeWaitConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = timeWaitCount,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_time_wait_connections",
        Name = MetricConfig.Tcp.TimeWaitConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect TCP connections in CLOSE_WAIT state
  /// </summary>
  private Domain.Metric CollectCloseWaitConnections()
  {
    try
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      var closeWaitCount = ipProperties.GetActiveTcpConnections()
        .Count(x => x.State == TcpState.CloseWait);

      var metric = new Domain.Metric
      {
        Type = "tcp_close_wait_connections",
        Name = MetricConfig.Tcp.CloseWaitConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = closeWaitCount,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_close_wait_connections",
        Name = MetricConfig.Tcp.CloseWaitConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect TCP connections in SYN state
  /// </summary>
  private Domain.Metric CollectSynConnections()
  {
    try
    {
      var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
      var synCount = ipProperties.GetActiveTcpConnections()
        .Count(x => x.State == TcpState.SynSent || x.State == TcpState.SynReceived);

      var metric = new Domain.Metric
      {
        Type = "tcp_syn_connections",
        Name = MetricConfig.Tcp.SynConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = synCount,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_syn_connections",
        Name = MetricConfig.Tcp.SynConnectionsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect TCP reset (RST) counter
  /// </summary>
  private Domain.Metric CollectTcpResets()
  {
    try
    {
      var tcpStats = IPGlobalProperties.GetIPGlobalProperties().GetTcpIPv4Statistics();
      var resets = tcpStats.ResetsSent;

      var metric = new Domain.Metric
      {
        Type = "tcp_resets",
        Name = MetricConfig.Tcp.ResetsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = resets,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "tcp_resets",
        Name = MetricConfig.Tcp.ResetsName,
        Unit = MetricConfig.Tcp.ConnectionsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }
}


using System.Net.NetworkInformation;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  /// <summary>
  /// Collect Disk usage metric in percentage
  /// </summary>
  private IEnumerable<Domain.Metric> CollectNetwork()
  {
    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
      .Where(ni => ni.OperationalStatus is OperationalStatus.Up &&
                   ni.NetworkInterfaceType is not NetworkInterfaceType.Loopback);

    foreach (var ni in interfaces)
    {
      var stats = ni.GetIPv4Statistics();
      var valueMb = stats.BytesReceived / (1024.0 * 1024);
      var metric = new Domain.Metric
      {
        Type = "network_traffic",
        Name = ni.Name,
        Unit = MetricConfig.Network.Unit,
        Metadata = new Dictionary<string, object>
        {
          { "bytesSent", stats.BytesSent },
          { "bytesReceived", stats.BytesReceived },
          { "speed", ni.Speed },
          { "interfaceType", ni.NetworkInterfaceType.ToString() }
        },
        TimestampUtc = DateTime.UtcNow,
        Value = Math.Round(valueMb, 2),
      };

      yield return metric;
    }
  }
}

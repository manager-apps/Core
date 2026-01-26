
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
      var value = stats.BytesReceived / (1024.0 * 1024);
      var metric = new Domain.Metric
      {
        Type = "network_usage",
        Name = MetricConfig.Network.Name,
        Unit = MetricConfig.Network.Unit,
        Metadata = new Dictionary<string, object>
        {
          { "bytesSent", stats.BytesSent },
          { "bytesReceived", stats.BytesReceived },
          { "speed", ni.Speed }
        },
        Value = value,
      };

      yield return metric;
    }
  }
}

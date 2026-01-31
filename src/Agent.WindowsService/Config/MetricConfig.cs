namespace Agent.WindowsService.Config;

public static class MetricConfig
{
  public static class Cpu
  {
    public const string CounterCategoryName = "Processor";
    public const string CounterName = "% Processor Time";
    public const string CounterInstanceName = "_Total";
    public const string Name = "Total Usage";
    public const string Unit = "percent";
  }

  public static class Memory
  {
    public const string CounterCategoryName = "Memory";
    public const string CounterName = "Available MBytes";
    public const string Name = "Available Memory";
    public const string Unit = "MB";
  }

  public static class Disk
  {
    public const string Name = "Disk Usage";
    public const string Unit = "percent";
  }

  public static class DiskIO
  {
    public const string CategoryName = "PhysicalDisk";
    public const string ReadBytesPerSecName = "Disk Read Bytes/sec";
    public const string WriteBytesPerSecName = "Disk Write Bytes/sec";
    public const string ReadSpeedName = "Disk Read Speed";
    public const string WriteSpeedName = "Disk Write Speed";
    public const string SpaceUnit = "percent";
    public const string SpeedUnit = "MB/s";
  }

  public static class Network
  {
    public const string Name = "Network Traffic";
    public const string Unit = "MB";
  }

  public static class Process
  {
    public const string ProcessCountName = "Total Processes";
    public const string ProcessCountUnit = "count";
    public const string CpuTimeUnit = "ms";
    public const string MemoryUnit = "MB";
    public const string HandleCountName = "Total Handles";
    public const string HandleCountUnit = "count";
    public const string ThreadCountName = "Total Threads";
    public const string ThreadCountUnit = "count";
  }

  public static class System
  {
    public const string UptimeName = "System Uptime";
    public const string UptimeUnit = "hours";
    public const string PhysicalMemoryName = "Managed Memory";
    public const string PhysicalMemoryUnit = "MB";
    public const string TimestampName = "System Timestamp";
    public const string TimestampUnit = "unix_seconds";
  }

  public static class Tcp
  {
    public const string TotalConnectionsName = "Total TCP Connections";
    public const string EstablishedConnectionsName = "Established Connections";
    public const string TimeWaitConnectionsName = "TIME_WAIT Connections";
    public const string CloseWaitConnectionsName = "CLOSE_WAIT Connections";
    public const string SynConnectionsName = "SYN Connections";
    public const string ResetsName = "TCP Resets";
    public const string ConnectionsUnit = "count";
  }

  public static class ApplicationHealth
  {
    public const string ErrorsName = "Application Errors";
    public const string WarningsName = "Application Warnings";
    public const string CriticalEventsName = "Critical Events";
    public const string InformationEventsName = "Information Events";
    public const string TotalEntriesName = "Total Event Log Entries";
    public const string HealthScoreName = "System Health Score";
    public const string EventsUnit = "count";
    public const string HealthScoreUnit = "score";
  }
}

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

  public static class Disk
  {
    public const string Name = "Disk Usage";
    public const string Unit = "percent";
  }

  public static class Memory
  {
    public const string CounterCategoryName = "Memory";
    public const string CounterName = "Available MBytes";
    public const string Name = "Available Memory";
    public const string Unit = "MB";
  }

  public static class Network
  {
    public const string Name = "Network Traffic";
    public const string Unit = "bytes";
  }
}

using System.Diagnostics;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  /// <summary>
  /// Collect total process count
  /// </summary>
  private Domain.Metric CollectProcessCount()
  {
    var processCount = Process.GetProcesses().Length;

    var metric = new Domain.Metric
    {
      Type = "process_count",
      Name = MetricConfig.Process.ProcessCountName,
      Unit = MetricConfig.Process.ProcessCountUnit,
      TimestampUtc = DateTime.UtcNow,
      Value = processCount,
    };

    return metric;
  }

  /// <summary>
  /// Collect top N processes by CPU usage
  /// </summary>
  private IEnumerable<Domain.Metric> CollectTopProcessesByCpu(int topCount = 5)
  {
    var metrics = new List<Domain.Metric>();

    try
    {
      var processes = Process.GetProcesses()
        .Where(p => p.ProcessName != string.Empty)
        .Select(p =>
        {
          try
          {
            return new
            {
              Process = p,
              TotalProcessorTime = p.TotalProcessorTime.TotalMilliseconds
            };
          }
          catch
          {
            // Skip processes where we don't have permission
            return null;
          }
        })
        .Where(x => x != null)
        .OrderByDescending(x => x.TotalProcessorTime)
        .Take(topCount)
        .ToList();

      foreach (var item in processes)
      {
        try
        {
          var metric = new Domain.Metric
          {
            Type = "process_cpu_time",
            Name = item.Process.ProcessName,
            Unit = MetricConfig.Process.CpuTimeUnit,
            TimestampUtc = DateTime.UtcNow,
            Value = Math.Round(item.TotalProcessorTime, 2),
            Metadata = new Dictionary<string, object>
            {
              { "processId", item.Process.Id },
              { "priority", item.Process.BasePriority },
              { "threads", item.Process.Threads.Count }
            }
          };

          metrics.Add(metric);
        }
        catch
        {
          // Skip if metric creation fails
        }
      }

      foreach (var item in processes)
      {
        try
        {
          item.Process?.Dispose();
        }
        catch
        {
          // Ignore disposal errors
        }
      }
    }
    catch
    {
      // Return empty list if collection fails
    }

    return metrics;
  }

  /// <summary>
  /// Collect top N processes by Memory usage
  /// </summary>
  private IEnumerable<Domain.Metric> CollectTopProcessesByMemory(int topCount = 5)
  {
    var metrics = new List<Domain.Metric>();

    try
    {
      var processes = Process.GetProcesses()
        .Where(p => p.ProcessName != string.Empty)
        .Select(p =>
        {
          try
          {
            return new
            {
              Process = p,
              MemoryMb = p.WorkingSet64 / (1024.0 * 1024)
            };
          }
          catch
          {
            // Skip processes where we don't have permission
            return null;
          }
        })
        .Where(x => x != null)
        .OrderByDescending(x => x.MemoryMb)
        .Take(topCount)
        .ToList();

      foreach (var item in processes)
      {
        try
        {
          var metric = new Domain.Metric
          {
            Type = "process_memory",
            Name = item.Process.ProcessName,
            Unit = MetricConfig.Process.MemoryUnit,
            TimestampUtc = DateTime.UtcNow,
            Value = Math.Round(item.MemoryMb, 2),
            Metadata = new Dictionary<string, object>
            {
              { "processId", item.Process.Id },
              { "workingSetBytes", item.Process.WorkingSet64 },
              { "virtualMemoryMb", Math.Round(item.Process.VirtualMemorySize64 / (1024.0 * 1024), 2) }
            }
          };

          metrics.Add(metric);
        }
        catch
        {
          // Skip if metric creation fails
        }
      }

      foreach (var item in processes)
      {
        try
        {
          item.Process?.Dispose();
        }
        catch
        {
          // Ignore disposal errors
        }
      }
    }
    catch
    {
      // Return empty list if collection fails
    }

    return metrics;
  }

  /// <summary>
  /// Collect handle count across all processes
  /// </summary>
  private Domain.Metric CollectTotalHandleCount()
  {
    try
    {
      var totalHandles = Process.GetProcesses()
        .Sum(p =>
        {
          try
          {
            return p.HandleCount;
          }
          catch
          {
            return 0;
          }
        });

      var metric = new Domain.Metric
      {
        Type = "total_handle_count",
        Name = MetricConfig.Process.HandleCountName,
        Unit = MetricConfig.Process.HandleCountUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = totalHandles,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "total_handle_count",
        Name = MetricConfig.Process.HandleCountName,
        Unit = MetricConfig.Process.HandleCountUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect total thread count across all processes
  /// </summary>
  private Domain.Metric CollectTotalThreadCount()
  {
    try
    {
      var totalThreads = Process.GetProcesses()
        .Sum(p =>
        {
          try
          {
            return p.Threads.Count;
          }
          catch
          {
            return 0;
          }
        });

      var metric = new Domain.Metric
      {
        Type = "total_thread_count",
        Name = MetricConfig.Process.ThreadCountName,
        Unit = MetricConfig.Process.ThreadCountUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = totalThreads,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "total_thread_count",
        Name = MetricConfig.Process.ThreadCountName,
        Unit = MetricConfig.Process.ThreadCountUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }
}

using System.Diagnostics;
using Agent.WindowsService.Config;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Infrastructure.Metric;

public partial class MetricCollector
{
  /// <summary>
  /// Collect application errors from Event Log (last hour)
  /// </summary>
  private Domain.Metric CollectApplicationErrors()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var oneHourAgo = DateTime.Now.AddHours(-1);

      var errorCount = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Error && e.TimeGenerated > oneHourAgo);

      var metric = new Domain.Metric
      {
        Type = "application_errors",
        Name = MetricConfig.ApplicationHealth.ErrorsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = errorCount,
        Metadata = new Dictionary<string, object>
        {
          { "timeRange", "last_hour" }
        }
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "application_errors",
        Name = MetricConfig.ApplicationHealth.ErrorsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect application warnings from Event Log (last hour)
  /// </summary>
  private Domain.Metric CollectApplicationWarnings()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var oneHourAgo = DateTime.Now.AddHours(-1);

      var warningCount = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Warning &&
                    e.TimeGenerated > oneHourAgo);

      var metric = new Domain.Metric
      {
        Type = "application_warnings",
        Name = MetricConfig.ApplicationHealth.WarningsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = warningCount,
        Metadata = new Dictionary<string, object>
        {
          { "timeRange", "last_hour" }
        }
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "application_warnings",
        Name = MetricConfig.ApplicationHealth.WarningsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect critical events from Event Log (last hour)
  /// </summary>
  private Domain.Metric CollectCriticalEvents()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var oneHourAgo = DateTime.Now.AddHours(-1);

      var criticalCount = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Error &&
                    (e.InstanceId >= 1000 || e.Source.Contains("Fatal")) &&
                    e.TimeGenerated > oneHourAgo);
      var metric = new Domain.Metric
      {
        Type = "critical_events",
        Name = MetricConfig.ApplicationHealth.CriticalEventsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = criticalCount,
        Metadata = new Dictionary<string, object>
        {
          { "timeRange", "last_hour" }
        }
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "critical_events",
        Name = MetricConfig.ApplicationHealth.CriticalEventsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect information events from Event Log (last hour)
  /// </summary>
  private Domain.Metric CollectInformationEvents()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var oneHourAgo = DateTime.Now.AddHours(-1);

      var infoCount = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Information &&
                    e.TimeGenerated > oneHourAgo);
      var metric = new Domain.Metric
      {
        Type = "information_events",
        Name = MetricConfig.ApplicationHealth.InformationEventsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = infoCount,
        Metadata = new Dictionary<string, object>
        {
          { "timeRange", "last_hour" }
        }
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "information_events",
        Name = MetricConfig.ApplicationHealth.InformationEventsName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Collect total event log entries count
  /// </summary>
  private Domain.Metric CollectTotalEventLogEntries()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var totalCount = eventLog.Entries.Count;
      var metric = new Domain.Metric
      {
        Type = "total_event_log_entries",
        Name = MetricConfig.ApplicationHealth.TotalEntriesName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = totalCount,
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "total_event_log_entries",
        Name = MetricConfig.ApplicationHealth.TotalEntriesName,
        Unit = MetricConfig.ApplicationHealth.EventsUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 0,
      };
    }
  }

  /// <summary>
  /// Calculate health score (0-100) based on events
  /// </summary>
  private Domain.Metric CollectHealthScore()
  {
    try
    {
      var eventLog = new EventLog("Application");
      var oneHourAgo = DateTime.Now.AddHours(-1);

      var errors = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Error &&
                    e.TimeGenerated > oneHourAgo);

      var warnings = eventLog.Entries
        .Cast<EventLogEntry>()
        .Count(e => e.EntryType == EventLogEntryType.Warning &&
                    e.TimeGenerated > oneHourAgo);

      // Health score: 100 - (errors * 5 + warnings * 1)
      var healthScore = Math.Max(0, 100 - (errors * 5 + warnings * 1));

      var metric = new Domain.Metric
      {
        Type = "health_score",
        Name = MetricConfig.ApplicationHealth.HealthScoreName,
        Unit = MetricConfig.ApplicationHealth.HealthScoreUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = healthScore,
        Metadata = new Dictionary<string, object>
        {
          { "errors", errors },
          { "warnings", warnings },
          { "timeRange", "last_hour" }
        }
      };

      return metric;
    }
    catch
    {
      return new Domain.Metric
      {
        Type = "health_score",
        Name = MetricConfig.ApplicationHealth.HealthScoreName,
        Unit = MetricConfig.ApplicationHealth.HealthScoreUnit,
        TimestampUtc = DateTime.UtcNow,
        Value = 100,
      };
    }
  }
}

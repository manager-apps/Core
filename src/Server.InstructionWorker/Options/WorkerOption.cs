namespace Server.InstructionWorker.Options;

public class WorkerOption
{
  public int PollingIntervalSeconds { get; set; }
  public int BatchSize { get; set; }
  public int MaxRetryCount { get; set; }
}

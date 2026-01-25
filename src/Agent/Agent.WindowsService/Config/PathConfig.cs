namespace Agent.WindowsService.Config;

public static class PathConfig
{
  /// <summary>
  /// Base directory in the common application data directory.
  /// </summary>
  public static readonly string BaseDirectory
    = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Manager");

  /// <summary>
  /// Metrics directory in the common application data directory.
  /// </summary>
  public static readonly string MetricsDirectory
    = Path.Combine(BaseDirectory, "metrics");

  /// <summary>
  /// Instructions directory in the common application data directory.
  /// </summary>
  public static readonly string InstructionsDirectory
    = Path.Combine(BaseDirectory, "instructions");

  /// <summary>
  /// Instruction results directory in the common application data directory.
  /// </summary>
  public static readonly string InstructionResultsDirectory
    = Path.Combine(InstructionsDirectory, "results");

  /// <summary>
  /// Log file path in the common application data directory.
  /// </summary>
  public static readonly string LogsFilePath
    = Path.Combine(BaseDirectory, "logs", "agent-.log");

  /// <summary>
  /// Config file path in the common application data directory.
  /// </summary>
  public static readonly string ConfigFilePath
    = Path.Combine(BaseDirectory, "config.json");

  /// <summary>
  /// Secret file path in the common application data directory.
  /// </summary>
  public static readonly string SecretFilePath
    = Path.Combine(BaseDirectory, "secrets.dat");

  /// <summary>
  /// SQLite metrics database connection string.
  /// </summary>
  public static readonly string MetricsConnectionString
    = $"Data Source={Path.Combine(BaseDirectory, "metrics.db")};Mode=ReadWriteCreate;Cache=Shared";

  /// <summary>
  /// SQLite instructions database connection string.
  /// </summary>
  public static readonly string InstructionsConnectionString
    = $"Data Source={Path.Combine(BaseDirectory, "instructions.db")};Mode=ReadWriteCreate;Cache=Shared";

  #region Fabrics

  /// <summary>
  /// Creates a new metric file path with a timestamp in the metrics directory.
  /// </summary>
  public static string CreateMetricFilePath
    => Path.Combine(MetricsDirectory, $"metrics-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json");

  /// <summary>
  /// Creates a new instruction result file path with a timestamp in the instructions directory.
  /// </summary>
  public static string CreateInstructionResultFilePath
    => Path.Combine(InstructionResultsDirectory, $"instruction-result-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json");

  /// <summary>
  /// Creates a new instruction file path with a timestamp in the instructions directory.
  /// </summary>
  public static string CreateInstructionFilePath
    => Path.Combine(InstructionsDirectory, $"instruction-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json");

  #endregion
}

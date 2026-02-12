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
}

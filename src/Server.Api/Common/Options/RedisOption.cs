namespace Server.Api.Common.Options;

public class RedisOption
{
  public const string SectionName = "Cache:Redis";
  public string InstanceName { get; init; } = string.Empty;
  public string ConnectionString { get; init; } = string.Empty;
}

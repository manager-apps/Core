namespace Server.Api.Features.Hardware;

/// <summary>
/// Response model for hardware information of an agent.
/// </summary>
public record HardwareResponse(
  long Id,
  long AgentId,
  string? OsVersion,
  string? MachineName,
  int ProcessorCount,
  long TotalMemoryBytes,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt);

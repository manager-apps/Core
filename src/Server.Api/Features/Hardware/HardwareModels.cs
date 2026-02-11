namespace Server.Api.Features.Hardware;

public record HardwareResponse(
  long Id,
  long AgentId,
  string? OsVersion,
  string? MachineName,
  int ProcessorCount,
  long TotalMemoryBytes,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt);

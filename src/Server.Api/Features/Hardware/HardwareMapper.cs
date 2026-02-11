namespace Server.Api.Features.Hardware;

internal static class HardwareMapper
{
  extension(Domain.Hardware hardware)
  {
    internal HardwareResponse ToResponse()
      => new(
        Id: hardware.Id,
        AgentId: hardware.AgentId,
        OsVersion: hardware.OsVersion,
        MachineName: hardware.MachineName,
        ProcessorCount: hardware.ProcessorCount,
        TotalMemoryBytes: hardware.TotalMemoryBytes,
        CreatedAt: hardware.CreatedAt,
        UpdatedAt: hardware.UpdatedAt);
  }
}

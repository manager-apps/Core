using Common.Messages;

namespace Server.Api.Features.Hardware;

public static class HardwareMapper
{
  extension(HardwareMessage hardware)
  {
    public Domain.Hardware ToDomain()
      => Domain.Hardware.Create(
          osVersion: hardware.OsVersion,
          machineName: hardware.MachineName,
          processorCount: hardware.ProcessorCount,
          totalMemoryBytes: hardware.TotalMemoryBytes);
  }

  extension(Domain.Hardware hardware)
  {
    public HardwareResponse ToResponse()
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

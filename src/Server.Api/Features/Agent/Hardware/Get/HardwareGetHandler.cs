using Microsoft.EntityFrameworkCore;
using Server.Api.Common.Result;
using Server.Api.Features.Hardware;
using Server.Api.Infrastructure;

namespace Server.Api.Features.Agent.Hardware.Get;

internal interface IHardwareGetHandler
{
  /// <summary>
  /// Handles the retrieval of an agent's hardware information
  /// </summary>
  Task<Result<HardwareResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken);
}

internal class HardwareGetHandler(
  AppDbContext dbContext
) : IHardwareGetHandler {
  public async Task<Result<HardwareResponse>> HandleAsync(
    long agentId,
    CancellationToken cancellationToken)
  {
    var agent = await dbContext.Agents
      .AsNoTracking()
      .Include(a => a.Hardware)
      .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
    if (agent is null)
      return AgentErrors.NotFound();

    return agent.Hardware.ToResponse();
  }
}

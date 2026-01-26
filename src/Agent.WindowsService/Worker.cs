using Agent.WindowsService.Abstraction;

namespace Agent.WindowsService;

public class Worker(IStateMachine stateMachine) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    => await Task.Delay(Timeout.Infinite, stoppingToken);

  public override async Task StartAsync(CancellationToken cancellationToken)
    => await stateMachine.StartAsync();

  public override async Task StopAsync(CancellationToken cancellationToken)
    => await stateMachine.StopAsync();
}

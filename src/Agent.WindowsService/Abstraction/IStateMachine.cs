namespace Agent.WindowsService.Abstraction;

public interface IStateMachine
{
  /// <summary>
  /// Starts the state machine.
  /// </summary>
  Task StartAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Stops the state machine.
  /// </summary>
  Task StopAsync(CancellationToken cancellationToken);
}

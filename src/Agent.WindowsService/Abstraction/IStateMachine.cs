namespace Agent.WindowsService.Abstraction;

public interface IStateMachine
{
  /// <summary>
  /// Starts the state machine.
  /// </summary>
  Task StartAsync();

  /// <summary>
  /// Stops the state machine.
  /// </summary>
  Task StopAsync();
}

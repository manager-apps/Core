using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleDelayingEntryAsync()
  {
    _logger.LogInformation("Entering Delaying state");
    try
    {
      await Task.Delay(500, Token);
      _logger.LogInformation("Delaying state completed successfully");

      await _machine.FireAsync(Triggers.Retry, Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Delaying state cancelled");
    }
  }
}

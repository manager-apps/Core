using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleSynchronizationEntryAsync()
  {
    _logger.LogInformation("Entering Synchronization state");
    try
    {
      var config = await _configStore.GetAsync(Token);

      _logger.LogInformation("Synchronization state completed successfully");
      await _machine.FireAsync(Triggers.SyncSuccess);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Synchronization state cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Synchronization state failed");
      await _machine.FireAsync(Triggers.SyncFailure);
    }
  }

  private async Task HandleSynchronizationExitAsync()
  {
    _logger.LogInformation("Exiting Synchronization state");
    try
    {
      await Task.Delay(5000, Token);
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Synchronization state exit delay cancelled");
    }
  }
}

using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine
{
  private async Task HandleSynchronizationEntryAsync()
  {
    _logger.LogInformation("Entering Synchronization state");
    try
    {
      var config = await _configStore.GetAsync(CancellationToken.None);

      // TODO: додати перевірку сумісності версій та синхронізацію часу

      _logger.LogInformation("Synchronization completed successfully");
      await _machine.FireAsync(Triggers.SyncSuccess);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Synchronization failed");
      await _machine.FireAsync(Triggers.SyncFailure);
    }
  }

  private async Task HandleSynchronizationExitAsync()
  {
    _logger.LogInformation("Exiting Synchronization state");

    // Delaying, will be configurable later
    await Task.Delay(5000);
  }
}

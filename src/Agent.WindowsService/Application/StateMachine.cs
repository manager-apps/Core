using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;

namespace Agent.WindowsService.Application;

public partial class StateMachine : IStateMachine
{
  private readonly Stateless.StateMachine<States, Triggers> _machine;
  private readonly IEnumerable<IInstructionExecutor> _executors;
  private readonly ILogger<StateMachine> _logger;
  private readonly IMetricCollector _metricCollector;
  private readonly IMetricStore _metricStore;
  private readonly IInstructionStore _instrStore;
  private readonly ISecretStore _secretStore;
  private readonly IServerClient _serverClient;
  private readonly IConfigurationStore _configStore;

  private CancellationTokenSource? _cts;
  private CancellationToken Token =>
    _cts?.Token ?? CancellationToken.None;

  public StateMachine(
    ILogger<StateMachine> logger,
    IEnumerable<IInstructionExecutor> executors,
    IMetricCollector metricCollector,
    IMetricStore metricStore,
    IInstructionStore instrStore,
    ISecretStore secretStore,
    IServerClient serverClient,
    IConfigurationStore configStore)
  {
    _logger = logger;
    _executors = executors;
    _metricCollector = metricCollector;
    _metricStore = metricStore;
    _instrStore = instrStore;
    _secretStore = secretStore;
    _serverClient = serverClient;
    _configStore = configStore;

    _machine = new Stateless.StateMachine<States, Triggers>(States.Idle, Stateless.FiringMode.Queued);
    ConfigureStateMachine();
  }

  private void ConfigureStateMachine()
  {
    _machine.Configure(States.Idle)
      .Permit(Triggers.Start, States.Authentication);

    _machine.Configure(States.Authentication)
      .OnEntryAsync(HandleAuthenticationEntryAsync)
      .OnExitAsync(HandleAuthenticationExitAsync)
      .Permit(Triggers.AuthSuccess, States.Synchronization)
      .Permit(Triggers.AuthFailure, States.Error)
      .Permit(Triggers.Stop, States.Idle);

    _machine.Configure(States.Running)
      .OnEntryAsync(HandleRunningEntryAsync)
      .OnExitAsync(HandleRunningExitAsync)
      .Permit(Triggers.AuthFailure, States.Authentication)
      .Permit(Triggers.RunSuccess, States.Execution)
      .Permit(Triggers.RunFailure, States.Error)
      .Permit(Triggers.Stop, States.Idle);

    _machine.Configure(States.Execution)
      .OnEntryAsync(HandleExecutionEntryAsync)
      .OnExitAsync(HandleExecutionExitAsync)
      .Permit(Triggers.ExecutionSuccess, States.Running)
      .Permit(Triggers.ExecutionFailure, States.Error)
      .Permit(Triggers.Stop, States.Idle);

    _machine.Configure(States.Error)
      .OnEntryAsync(HandleDelayingEntryAsync)
      .Permit(Triggers.Retry, States.Running)
      .Permit(Triggers.Stop, States.Idle);
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _cts?.Dispose();
    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    await _machine.FireAsync(Triggers.Start, Token);
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await _cts?.CancelAsync()!;
    await _machine.FireAsync(Triggers.Stop, cancellationToken);
  }
}

namespace Agent.WindowsService.Domain;

public enum Triggers
{
  Start,
  Stop,
  Retry,
  AuthSuccess,
  AuthFailure,
  SyncSuccess,
  SyncFailure,
  RunSuccess,
  RunFailure,
  ExecutionSuccess,
  ExecutionFailure,
  DelaySuccess,
  DelayFailure
}

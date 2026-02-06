namespace Agent.WindowsService.Domain;

public enum Triggers
{
  Start,
  Stop,
  Retry,
  AuthSuccess,
  AuthFailure,
  RunSuccess,
  RunFailure,
  ExecutionSuccess,
  ExecutionFailure
}

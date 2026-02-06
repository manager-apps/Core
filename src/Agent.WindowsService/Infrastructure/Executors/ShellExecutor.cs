using System.Diagnostics;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;
using Common.Messages;
using FluentValidation;

namespace Agent.WindowsService.Infrastructure.Executors;

public class ShellExecutor(IValidator<Instruction> validator) : IInstructionExecutor
{
  public bool CanExecute(InstructionType type) => type is InstructionType.Shell;

  public async Task<InstructionResult> ExecuteAsync(Instruction instruction, CancellationToken cancellationToken = default)
  {
    var isValid = await validator.ValidateAsync(instruction, cancellationToken);
    if (!isValid.IsValid)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Output = string.Join("; ", isValid.Errors.Select(e => e.ErrorMessage)),
        Error = "Validation failed"
      };
    }

    if (instruction.Payload is not ShellCommandPayload shellPayload)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Error = $"Invalid payload type: expected {nameof(ShellCommandPayload)}"
      };
    }

    using var process = new Process();
    process.StartInfo = new ProcessStartInfo
    {
      FileName = "cmd.exe",
      Arguments = $"/c {shellPayload.Command}",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      WorkingDirectory = Environment.SystemDirectory
    };

    var output = new System.Text.StringBuilder();
    var error = new System.Text.StringBuilder();

    process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    var exited = await Task.Run(() => process.WaitForExit(shellPayload.Timeout), cancellationToken);
    if (!exited)
    {
      try { process.Kill(entireProcessTree: true); } catch { /*ignored*/ }
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Output = output.ToString(),
        Error = "Process timed out"
      };
    }

    return new InstructionResult
    {
      AssociativeId = instruction.AssociativeId,
      Success = process.ExitCode == 0,
      Output = output.ToString(),
      Error = error.ToString()
    };
  }
}

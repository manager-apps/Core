using System.Diagnostics;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;
using Common.Messages;
using FluentValidation;
using Microsoft.Win32;

namespace Agent.WindowsService.Infrastructure.Executors;

public class GpoExecutor(IValidator<Instruction> validator) : IInstructionExecutor
{
  public bool CanExecute(InstructionType type) => type == InstructionType.Gpo;

  public async Task<InstructionResult> ExecuteAsync(
    Instruction instruction,
    CancellationToken cancellationToken = default)
  {
    var validationResult = await validator.ValidateAsync(instruction, cancellationToken);
    if (!validationResult.IsValid)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Output = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
        Error = "Validation failed"
      };
    }

    if (instruction.Payload is not GpoSetPayload gpoPayload)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Error = $"Invalid payload type: expected {nameof(GpoSetPayload)}"
      };
    }

    try
    {
      SetPolicy(gpoPayload);

      using var process = new Process();
      process.StartInfo = new ProcessStartInfo
      {
        FileName = "gpupdate",
        Arguments = "/force",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      var output = new System.Text.StringBuilder();
      var error = new System.Text.StringBuilder();

      process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
      process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      const int timeout = 10000;
      var exited = await Task.Run(() => process.WaitForExit(timeout), cancellationToken);
      if (!exited)
      {
        try { process.Kill(entireProcessTree: true); } catch { /*ignored*/ }

        return new InstructionResult
        {
          AssociativeId = instruction.AssociativeId,
          Success = false,
          Output = output.ToString(),
          Error = "gpupdate process timed out"
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
    catch (Exception ex)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Output = "",
        Error = $"Failed to set registry policy: {ex.Message}"
      };
    }
  }

  private static void SetPolicy(GpoSetPayload payload)
  {
    // For now, GpoSetPayload uses simple Name and Value
    // If you need more complex GPO operations, extend GpoSetPayload
    var rootKey = Registry.LocalMachine;
    using var key = rootKey.CreateSubKey(@"Software\Policies")
      ?? throw new InvalidOperationException("Failed to create or open registry key");

    key.SetValue(payload.Name, payload.Value, RegistryValueKind.String);
  }
}

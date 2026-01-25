using System.Diagnostics;
using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;
using FluentValidation;
using Microsoft.Win32;

namespace Agent.WindowsService.Infrastructure.Executors;

public class GpoExecutor(IValidator<Instruction> validator) : IInstructionExecutor
{
  public bool CanExecute(InstructionType type) => type == InstructionType.GpoSet;

  public async Task<InstructionResult> ExecuteAsync(Instruction instruction, CancellationToken cancellationToken = default)
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

    var name = instruction.Payload.GetValueOrDefault("name")!;
    var valueType = instruction.Payload.GetValueOrDefault("valueType")!;
    var value = instruction.Payload.GetValueOrDefault("value")!;
    var registryKeyType = instruction.Payload.GetValueOrDefault("registryKeyType")!;
    var registryPath = instruction.Payload.GetValueOrDefault("registryPath")!;

    try
    {
      SetPolicy(registryKeyType, registryPath, name, valueType, value);

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

      var timeout = 10000;
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

  private static void SetPolicy(string scope, string path, string name, string valueType, string value)
  {
    var rootKey = GetRegistryKey(scope);
    using var key = rootKey.CreateSubKey(path)
              ?? throw new InvalidOperationException($"Failed to create or open registry key: {path}");

    var regValueType = GetRegistryValueType(valueType);
    var convertedValue = ConvertValue(value, regValueType);

    key.SetValue(name, convertedValue, regValueType);
  }

  private static RegistryKey GetRegistryKey(string scope) => scope.ToLowerInvariant() switch
  {
    "machine" => Registry.LocalMachine,
    "user" => Registry.CurrentUser,
    "users" => Registry.Users,
    _ => throw new ArgumentException($"Unknown registry scope: {scope}")
  };

  private static RegistryValueKind GetRegistryValueType(string valueType) => valueType.ToLowerInvariant() switch
  {
    "dword" => RegistryValueKind.DWord,
    "qword" => RegistryValueKind.QWord,
    "string" => RegistryValueKind.String,
    "expandstring" => RegistryValueKind.ExpandString,
    "multistring" => RegistryValueKind.MultiString,
    "binary" => RegistryValueKind.Binary,
    _ => throw new ArgumentException($"Unknown registry value type: {valueType}")
  };

  private static object ConvertValue(string value, RegistryValueKind valueType) => valueType switch
  {
    RegistryValueKind.DWord => int.TryParse(value, out var intValue) ? intValue : throw new ArgumentException($"Invalid DWord value: {value}"),
    RegistryValueKind.QWord => long.TryParse(value, out var longValue) ? longValue : throw new ArgumentException($"Invalid QWord value: {value}"),
    RegistryValueKind.MultiString => value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries),
    RegistryValueKind.Binary => Convert.FromBase64String(value),
    _ => value
  };
}

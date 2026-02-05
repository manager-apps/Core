using Agent.WindowsService.Abstraction;
using Agent.WindowsService.Domain;
using Common.Messages;
using FluentValidation;
using Serilog;

namespace Agent.WindowsService.Infrastructure.Executors;

public class ConfigExecutor(
  IValidator<Instruction> validator,
  IConfigurationStore configStore
) : IInstructionExecutor {
  public bool CanExecute(InstructionType type) => type == InstructionType.Config;

  public async Task<InstructionResult> ExecuteAsync(
    Instruction instruction,
    CancellationToken cancellationToken)
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

    if (instruction.Payload is not ConfigPayload configPayload)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Error = $"Invalid payload type: expected {nameof(ConfigPayload)}"
      };
    }

    try
    {
      var currentConfig = await configStore.GetAsync(cancellationToken);
      var newConfig = configPayload.Config;
      var updatedConfig = currentConfig with
      {
        AuthenticationExitIntervalSeconds = newConfig.AuthenticationExitIntervalSeconds,
        SynchronizationExitIntervalSeconds = newConfig.SynchronizationExitIntervalSeconds,
        InstructionsExecutionLimit = newConfig.InstructionsExecutionLimit,
        RunningExitIntervalSeconds = newConfig.RunningExitIntervalSeconds,
        ExecutionExitIntervalSeconds = newConfig.ExecutionExitIntervalSeconds,
        InstructionResultsSendLimit = newConfig.InstructionResultsSendLimit,
        MetricsSendLimit = newConfig.MetricsSendLimit,
        AllowedCollectors = newConfig.AllowedCollectors,
        AllowedInstructions = newConfig.AllowedInstructions
      };
      await configStore.SaveAsync(updatedConfig, cancellationToken);

      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = true,
        Output = "Configuration updated successfully"
      };
    }
    catch (Exception ex)
    {
      return new InstructionResult
      {
        AssociativeId = instruction.AssociativeId,
        Success = false,
        Error = $"Failed to update configuration: {ex.Message}"
      };
    }
  }
}

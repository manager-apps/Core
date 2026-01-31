using Agent.WindowsService.Domain;
using Common.Messages;
using FluentValidation;

namespace Agent.WindowsService.Validators;

public class InstructionValidator : AbstractValidator<Instruction>
{
  public InstructionValidator()
  {
    RuleFor(instruction => instruction.Payload)
      .NotNull()
      .WithMessage("Payload cannot be null.");

    RuleFor(instruction => instruction.Payload)
      .Must((instruction, _) => instruction.Payload is ShellCommandPayload)
      .WithMessage("Payload must be ShellCommandPayload for ShellCommand instructions.")
      .When(instruction => instruction.Type == InstructionType.ShellCommand);

    RuleFor(instruction => instruction.Payload)
      .Must((instruction, _) => 
      {
        if (instruction.Payload is not ShellCommandPayload shellPayload)
          return false;
        
        return !string.IsNullOrWhiteSpace(shellPayload.Command);
      })
      .WithMessage("The 'Command' field must be a non-empty string.")
      .When(instruction => instruction.Type == InstructionType.ShellCommand);

    RuleFor(instruction => instruction.Payload)
      .Must((instruction, _) => instruction.Payload is GpoSetPayload)
      .WithMessage("Payload must be GpoSetPayload for GpoSet instructions.")
      .When(instruction => instruction.Type == InstructionType.GpoSet);

    RuleFor(instruction => instruction.Payload)
      .Must((instruction, _) =>
      {
        if (instruction.Payload is not GpoSetPayload gpoPayload)
          return false;

        return !string.IsNullOrWhiteSpace(gpoPayload.Name) &&
               !string.IsNullOrWhiteSpace(gpoPayload.Value);
      })
      .WithMessage("The 'Name' and 'Value' fields must be non-empty strings.")
      .When(instruction => instruction.Type == InstructionType.GpoSet);
  }
}

using FluentValidation;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.CreateSalaryStructure;

public class CreateSalaryStructureCommandValidator : AbstractValidator<CreateSalaryStructureCommand>
{
    public CreateSalaryStructureCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^[A-Z0-9_-]{2,50}$")
            .WithMessage("Code must be uppercase alphanumeric (e.g. EXEC_STRUCTURE_2026).");

        RuleFor(x => x.EffectiveTo)
            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo cannot be earlier than EffectiveFrom.");
    }
}

using FluentValidation;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetAllowanceRule;

public class SetAllowanceRuleCommandValidator : AbstractValidator<SetAllowanceRuleCommand>
{
    public SetAllowanceRuleCommandValidator()
    {
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.StructureComponentId).NotEmpty();

        RuleFor(x => x.ApplicationMode)
            .NotEmpty()
            .Must(BeAValidApplicationMode)
            .WithMessage("Invalid allowance application mode. Allowed: Automatic, ConditionBased, ManualClaim.");
    }

    private static bool BeAValidApplicationMode(string mode) =>
        Enum.TryParse<AllowanceApplicationMode>(mode, true, out _);
}

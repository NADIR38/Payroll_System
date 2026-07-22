using FluentValidation;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetDeductionRule;

public class SetDeductionRuleCommandValidator : AbstractValidator<SetDeductionRuleCommand>
{
    public SetDeductionRuleCommandValidator()
    {
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.StructureComponentId).NotEmpty();

        RuleFor(x => x.DeductionType)
            .NotEmpty()
            .Must(BeAValidDeductionType)
            .WithMessage("Invalid deduction type. Allowed: Standard, LateArrival, Absence, LoanRepayment, AdvanceRecovery, CustomPenalty.");

        RuleFor(x => x.GracePeriodMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Grace period minutes must be non-negative.");
    }

    private static bool BeAValidDeductionType(string type) =>
        Enum.TryParse<DeductionType>(type, true, out _);
}

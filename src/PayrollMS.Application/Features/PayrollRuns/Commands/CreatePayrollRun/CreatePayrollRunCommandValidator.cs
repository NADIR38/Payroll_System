using FluentValidation;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.CreatePayrollRun;

public sealed class CreatePayrollRunCommandValidator : AbstractValidator<CreatePayrollRunCommand>
{
    public CreatePayrollRunCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId is required.");

        RuleFor(x => x.FinancialYearId)
            .NotEmpty().WithMessage("FinancialYearId is required.");

        RuleFor(x => x.PeriodYear)
            .InclusiveBetween(2000, 2100).WithMessage("PeriodYear is out of range.");

        RuleFor(x => x.PeriodMonth)
            .InclusiveBetween(1, 12).WithMessage("PeriodMonth must be between 1 and 12.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required.");
    }
}

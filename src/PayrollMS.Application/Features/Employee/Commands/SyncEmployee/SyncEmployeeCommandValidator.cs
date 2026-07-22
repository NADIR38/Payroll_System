using FluentValidation;

namespace PayrollMS.Application.Features.Employee.Commands.SyncEmployee;

public class SyncEmployeeCommandValidator : AbstractValidator<SyncEmployeeCommand>
{
    public SyncEmployeeCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ExternalEmployeeId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmployeeCode).MaximumLength(50);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DesignationId).NotEmpty();
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.BaseSalary).GreaterThanOrEqualTo(0)
            .WithMessage("Base salary cannot be negative.");

        When(x => !string.IsNullOrWhiteSpace(x.IBAN), () =>
        {
            RuleFor(x => x.BankName).NotEmpty().WithMessage("Bank name is required when IBAN is provided.");
            RuleFor(x => x.AccountTitle).NotEmpty().WithMessage("Account title is required when IBAN is provided.");
            RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account number is required when IBAN is provided.");
        });
    }
}

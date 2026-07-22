using FluentValidation;

namespace PayrollMS.Application.Features.Employee.Commands.AddEmployeeBankAccount;

public class AddEmployeeBankAccountCommandValidator : AbstractValidator<AddEmployeeBankAccountCommand>
{
    public AddEmployeeBankAccountCommandValidator()
    {
        RuleFor(x => x.EmployeePayrollProfileId).NotEmpty();
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AccountTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.IBAN).NotEmpty().MaximumLength(50);
    }
}

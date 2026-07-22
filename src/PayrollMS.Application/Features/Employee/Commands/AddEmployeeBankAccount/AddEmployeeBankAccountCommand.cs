using MediatR;

namespace PayrollMS.Application.Features.Employee.Commands.AddEmployeeBankAccount;

public sealed record AddEmployeeBankAccountCommand(
    Guid EmployeePayrollProfileId,
    string BankName,
    string AccountTitle,
    string AccountNumber,
    string IBAN,
    string? BranchCode,
    bool IsPrimary) : IRequest<Guid>;

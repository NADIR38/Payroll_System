using MediatR;

namespace PayrollMS.Application.Features.Employee.Commands.SetPrimaryBankAccount;

public sealed record SetPrimaryBankAccountCommand(
    Guid EmployeePayrollProfileId,
    Guid BankAccountId) : IRequest;

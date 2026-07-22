using MediatR;

namespace PayrollMS.Application.Features.Employee.Commands.SyncEmployee;

public sealed record SyncEmployeeCommand(
    Guid CompanyId,
    string ExternalEmployeeId,
    string FullName,
    Guid BranchId,
    Guid DepartmentId,
    Guid DesignationId,
    Guid? CostCenterId,
    Guid SalaryStructureId,
    decimal BaseSalary,
    DateOnly JoiningDate,
    bool AttendanceDeductionOptIn = true,
    string? EmployeeCode = null,
    string? BankName = null,
    string? AccountTitle = null,
    string? AccountNumber = null,
    string? IBAN = null,
    string? BranchCode = null,
    string? SyncedBy = null) : IRequest<SyncEmployeeResult>;

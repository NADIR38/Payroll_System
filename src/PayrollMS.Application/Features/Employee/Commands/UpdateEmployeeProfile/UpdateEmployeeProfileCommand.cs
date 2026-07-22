using MediatR;

namespace PayrollMS.Application.Features.Employee.Commands.UpdateEmployeeProfile;

public sealed record UpdateEmployeeProfileCommand(
    Guid Id,
    string FullName,
    Guid BranchId,
    Guid DepartmentId,
    Guid DesignationId,
    Guid? CostCenterId,
    Guid SalaryStructureId,
    decimal BaseSalary,
    DateOnly JoiningDate,
    DateOnly? LeavingDate,
    bool AttendanceDeductionOptIn,
    string Status,
    string ChangedBy,
    string ChangeReason) : IRequest;

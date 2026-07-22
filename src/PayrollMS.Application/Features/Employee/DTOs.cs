namespace PayrollMS.Application.Features.Employee;

/// <summary>
/// Employee Bank Account Response DTO.
/// </summary>
public sealed record EmployeeBankAccountResponse(
    Guid Id,
    Guid EmployeePayrollProfileId,
    string BankName,
    string AccountTitle,
    string AccountNumber,
    string IBAN,
    string? BranchCode,
    bool IsPrimary,
    bool IsActive);

/// <summary>
/// Employee Profile History Temporal Response DTO.
/// </summary>
public sealed record EmployeeProfileHistoryResponse(
    Guid Id,
    Guid EmployeePayrollProfileId,
    string ExternalEmployeeId,
    string EmployeeCode,
    string FullName,
    Guid SalaryStructureId,
    decimal BaseSalary,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? ChangedBy,
    string? ChangeReason);

/// <summary>
/// Employee Payroll Profile Response DTO.
/// </summary>
public sealed record EmployeeProfileResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalEmployeeId,
    string EmployeeCode,
    string FullName,
    Guid BranchId,
    Guid DepartmentId,
    Guid DesignationId,
    Guid? CostCenterId,
    Guid SalaryStructureId,
    decimal BaseSalary,
    bool AttendanceDeductionOptIn,
    DateOnly JoiningDate,
    DateOnly? LeavingDate,
    string Status,
    DateOnly EffectiveFrom,
    DateTimeOffset CreatedAt,
    IReadOnlyList<EmployeeBankAccountResponse> BankAccounts);

/// <summary>
/// Result returned from SyncEmployeeCommand.
/// Action = "Created" | "Updated".
/// </summary>
public sealed record SyncEmployeeResult(
    Guid ProfileId,
    string Action);

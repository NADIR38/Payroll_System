namespace PayrollMS.Application.Features.PayrollRuns;

public sealed record PayrollRunResponse(
    Guid Id,
    Guid CompanyId,
    Guid FinancialYearId,
    int PeriodYear,
    int PeriodMonth,
    string Status,
    string RunType,
    Guid? FilterBranchId,
    Guid? FilterDepartmentId,
    int TotalEmployees,
    decimal TotalGross,
    decimal TotalDeductions,
    decimal TotalNet,
    DateTimeOffset? GeneratedAt,
    string? GeneratedBy,
    string? Remarks,
    int RunVersion,
    Guid? ParentRunId,
    DateTimeOffset CreatedAt);

public sealed record PayrollEntryComponentResponse(
    Guid Id,
    Guid PayrollEntryId,
    Guid SalaryComponentId,
    string ComponentCode,
    string ComponentName,
    string ComponentType,
    string FormulaUsed,
    decimal CalculatedAmount,
    bool IsManualOverride);

public sealed record PayrollEntryResponse(
    Guid Id,
    Guid PayrollRunId,
    Guid CompanyId,
    string ExternalEmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string DepartmentName,
    string DesignationName,
    string? BankName,
    string? IBAN,
    decimal BaseSalary,
    int WorkingDays,
    int AbsentDays,
    int LateDays,
    decimal GrossSalary,
    decimal TotalDeductions,
    decimal NetSalary,
    string Status,
    IReadOnlyList<PayrollEntryComponentResponse> Components);

public sealed record PayrollRunCreatedResult(
    Guid PayrollRunId,
    string Status,
    string Message);

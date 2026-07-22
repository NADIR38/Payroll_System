using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Entities.Salary;

namespace PayrollMS.Domain.Interfaces.Services;

/// <summary>
/// Orchestrates the full salary calculation for a single employee for a given payroll period.
/// Called by the Hangfire PayrollGenerationWorker per employee — PRD §15.5
///
/// FormulaContext is defined in IFormulaEvaluator.cs (same namespace).
/// </summary>
public interface IPayrollCalculator
{
    /// <summary>
    /// Builds a FormulaContext, evaluates all SalaryStructureComponents in sequence order,
    /// applies AllowanceRules and DeductionRules, and returns a populated PayrollEntry.
    ///
    /// If Attendance is null → returns entry with Status = AttendanceMissing.
    /// If Structure is null → returns entry with Status = StructureMissing.
    /// PRD §15.6, §10.4
    /// </summary>
    Task<PayrollEntry> CalculateAsync(
        PayrollCalculationInput input,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// All data required to calculate a single employee's payroll entry.
/// Passed as a value object to IPayrollCalculator.CalculateAsync.
/// </summary>
public sealed class PayrollCalculationInput
{
    public required PayrollRunId PayrollRunId { get; init; }
    public required CompanyId CompanyId { get; init; }

    // ── Employee snapshot data ────────────────────────────────────────────────

    public required string ExternalEmployeeId { get; init; }
    public required string EmployeeCode { get; init; }
    public required string EmployeeName { get; init; }
    public required string DepartmentName { get; init; }
    public required string DepartmentCode { get; init; }
    public required string DesignationName { get; init; }
    public required string DesignationCode { get; init; }

    /// <summary>From the employee's primary EmployeeBankAccount — snapshotted into PayrollEntry.</summary>
    public required string? BankName { get; init; }
    public required string? IBAN { get; init; }

    public required decimal BaseSalary { get; init; }

    /// <summary>
    /// When false and a DeductionRule has IsOptIn = true, attendance deductions are skipped.
    /// PRD §12.4
    /// </summary>
    public required bool AttendanceDeductionOptIn { get; init; }

    public required int PeriodYear { get; init; }
    public required int PeriodMonth { get; init; }

    // ── Dependencies ──────────────────────────────────────────────────────────

    /// <summary>
    /// The salary structure to evaluate. If null, entry is created with StructureMissing status.
    /// Must be the version active on the period start date — PRD §9.4
    /// </summary>
    public required SalaryStructure? Structure { get; init; }

    /// <summary>
    /// Attendance for the period. If null, entry is created with AttendanceMissing status.
    /// PRD §15.6
    /// </summary>
    public required AttendanceSummary? Attendance { get; init; }

    /// <summary>
    /// Leave summary for the period. Null is acceptable — treated as zero leave days.
    /// </summary>
    public required LeaveSummary? Leave { get; init; }

    /// <summary>Authoritative working days from PayrollCalendar — PRD §6.4</summary>
    public required int CalendarWorkingDays { get; init; }

    /// <summary>Active loan installment amount for this period (0 if no active loan).</summary>
    public required decimal LoanInstallmentAmount { get; init; }

    /// <summary>Active advance recovery amount for this period (0 if none).</summary>
    public required decimal AdvanceRecoveryAmount { get; init; }
}

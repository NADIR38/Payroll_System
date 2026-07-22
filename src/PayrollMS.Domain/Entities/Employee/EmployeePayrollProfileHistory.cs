using PayrollMS.Domain.Common;

namespace PayrollMS.Domain.Entities.Employee;

/// <summary>
/// Immutable temporal snapshot of an EmployeePayrollProfile.
/// Written whenever the profile is created or updated.
/// Payroll generation reads the version with EffectiveTo = null or EffectiveTo >= period start.
/// PRD §7.4 — "payroll generation uses the profile version active on the payroll period's start date"
/// </summary>
public sealed class EmployeePayrollProfileHistory : BaseEntity<EmployeePayrollProfileHistoryId>
{
    private EmployeePayrollProfileHistory() { }

    public EmployeePayrollProfileId EmployeePayrollProfileId { get; private set; }
    public CompanyId CompanyId { get; private set; }

    // Snapshot fields (mirroring key profile values at time of change)
    public string ExternalEmployeeId { get; private set; } = null!;
    public string EmployeeCode { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public SalaryStructureId SalaryStructureId { get; private set; }
    public decimal BaseSalary { get; private set; }

    // Temporal range — null EffectiveTo means "this is the current version"
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    public string? ChangedBy { get; private set; }
    public string? ChangeReason { get; private set; }

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>Creates the initial history row on first profile sync (action = "Created").</summary>
    internal static EmployeePayrollProfileHistory CreateInitial(
        EmployeePayrollProfileId profileId,
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string fullName,
        SalaryStructureId salaryStructureId,
        decimal baseSalary,
        DateOnly effectiveFrom,
        string? changedBy)
    {
        return new EmployeePayrollProfileHistory
        {
            Id = EmployeePayrollProfileHistoryId.New(),
            EmployeePayrollProfileId = profileId,
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId,
            EmployeeCode = employeeCode,
            FullName = fullName,
            SalaryStructureId = salaryStructureId,
            BaseSalary = baseSalary,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            ChangedBy = changedBy,
            ChangeReason = "Initial sync"
        };
    }

    /// <summary>Creates a new history row when the profile is updated.</summary>
    internal static EmployeePayrollProfileHistory Create(
        EmployeePayrollProfileId profileId,
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string fullName,
        SalaryStructureId salaryStructureId,
        decimal baseSalary,
        DateOnly effectiveFrom,
        string changedBy,
        string changeReason)
    {
        return new EmployeePayrollProfileHistory
        {
            Id = EmployeePayrollProfileHistoryId.New(),
            EmployeePayrollProfileId = profileId,
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId,
            EmployeeCode = employeeCode,
            FullName = fullName,
            SalaryStructureId = salaryStructureId,
            BaseSalary = baseSalary,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            ChangedBy = changedBy,
            ChangeReason = changeReason
        };
    }

    /// <summary>
    /// Closes this history row by setting EffectiveTo.
    /// Called by EmployeePayrollProfile.Update() before opening a new row.
    /// </summary>
    internal void Close(DateOnly effectiveTo)
    {
        EffectiveTo = effectiveTo;
    }

    internal void UpdateInPlace(
        string fullName,
        SalaryStructureId salaryStructureId,
        decimal baseSalary,
        string changedBy,
        string changeReason)
    {
        FullName = fullName.Trim();
        SalaryStructureId = salaryStructureId;
        BaseSalary = baseSalary;
        ChangedBy = changedBy;
        ChangeReason = changeReason;
        SetUpdatedAt();
    }
}

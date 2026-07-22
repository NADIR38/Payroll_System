using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Payroll;

/// <summary>
/// A single employee's salary calculation result within a PayrollRun.
/// Every string/numeric field is an immutable snapshot taken at generation time — PRD §2.4, §15.2
///
/// Hard rules (AGENTS.md):
///   - Never edit components once PayrollRun.Status &gt; GENERATED
///   - Override is only allowed via OverrideComponent() which enforces the status guard
/// </summary>
public sealed class PayrollEntry : BaseEntity<PayrollEntryId>
{
    private readonly List<PayrollEntryComponent> _components = [];

    private PayrollEntry() { }

    public PayrollRunId PayrollRunId { get; private set; }
    public CompanyId CompanyId { get; private set; }

    // ── Snapshot fields — copied from EmployeePayrollProfile at generation time ──

    /// <summary>ERP identifier — snapshot. PRD §15.2</summary>
    public string ExternalEmployeeId { get; private set; } = null!;
    public string EmployeeCode { get; private set; } = null!;
    public string EmployeeName { get; private set; } = null!;
    public string DepartmentName { get; private set; } = null!;
    public string DesignationName { get; private set; } = null!;

    /// <summary>Primary bank — snapshot from EmployeeBankAccount at generation time.</summary>
    public string? BankName { get; private set; }
    public string? IBAN { get; private set; }

    /// <summary>Employee's BaseSalary at the time of the run — snapshot.</summary>
    public decimal BaseSalary { get; private set; }

    // ── Attendance snapshot — from AttendanceSummary ──

    public int WorkingDays { get; private set; }
    public int AbsentDays { get; private set; }
    public int LateDays { get; private set; }

    // ── Calculated totals ──

    /// <summary>Sum of all Earning components. NUMERIC(12,2) — PRD §27.3</summary>
    public decimal GrossSalary { get; private set; }
    public decimal TotalDeductions { get; private set; }
    public decimal NetSalary { get; private set; }

    public PayrollEntryStatus Status { get; private set; }

    public IReadOnlyCollection<PayrollEntryComponent> Components => _components.AsReadOnly();

    // ── Factory: successful calculation ──────────────────────────────────────

    public static PayrollEntry CreateCalculated(
        PayrollRunId runId,
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string employeeName,
        string departmentName,
        string designationName,
        string? bankName,
        string? iban,
        decimal baseSalary,
        int workingDays,
        int absentDays,
        int lateDays,
        decimal grossSalary,
        decimal totalDeductions,
        IEnumerable<PayrollEntryComponent> components)
    {
        var entry = new PayrollEntry
        {
            Id = PayrollEntryId.New(),
            PayrollRunId = runId,
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId,
            EmployeeCode = employeeCode,
            EmployeeName = employeeName,
            DepartmentName = departmentName,
            DesignationName = designationName,
            BankName = bankName,
            IBAN = iban,
            BaseSalary = baseSalary,
            WorkingDays = workingDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            GrossSalary = grossSalary,
            TotalDeductions = totalDeductions,
            NetSalary = grossSalary - totalDeductions,
            Status = PayrollEntryStatus.Calculated
        };

        entry._components.AddRange(components);
        return entry;
    }

    // ── Factory: error states ─────────────────────────────────────────────────

    public static PayrollEntry CreateWithError(
        PayrollRunId runId,
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string employeeName,
        PayrollEntryStatus errorStatus)
    {
        if (errorStatus is not (PayrollEntryStatus.AttendanceMissing or PayrollEntryStatus.StructureMissing))
            throw new BusinessRuleViolationException("InvalidErrorStatus",
                "Only AttendanceMissing or StructureMissing are valid error states at creation.");

        return new PayrollEntry
        {
            Id = PayrollEntryId.New(),
            PayrollRunId = runId,
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId,
            EmployeeCode = employeeCode,
            EmployeeName = employeeName,
            DepartmentName = string.Empty,
            DesignationName = string.Empty,
            BaseSalary = 0,
            GrossSalary = 0,
            TotalDeductions = 0,
            NetSalary = 0,
            Status = errorStatus
        };
    }

    // ── Manual component override ─────────────────────────────────────────────

    /// <summary>
    /// Allows HR to manually override a single component amount before approval.
    /// PRD §15.7 — guard: only when run is GENERATED or UNDER_REVIEW.
    /// The caller (command handler) must pass the run's current status.
    /// </summary>
    public PayrollEntryComponent OverrideComponent(
        PayrollEntryComponentId componentId,
        decimal newAmount,
        string overriddenBy,
        string reason,
        PayrollRunStatus runStatus)
    {
        // Hard rule from AGENTS.md: never edit past GENERATED
        if (runStatus is not (PayrollRunStatus.Generated or PayrollRunStatus.UnderReview))
            throw new PayrollFrozenException(nameof(PayrollEntry), Id.ToString()!);

        if (string.IsNullOrWhiteSpace(overriddenBy))
            throw new BusinessRuleViolationException("OverriddenByRequired", "OverriddenBy is required for component override.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("ReasonRequired", "A reason is required for component override.");

        var component = _components.FirstOrDefault(c => c.Id == componentId)
            ?? throw new BusinessRuleViolationException("ComponentNotFound",
                $"PayrollEntryComponent '{componentId}' not found on this entry.");

        var oldAmount = component.CalculatedAmount;
        component.ApplyOverride(newAmount);

        // Recalculate totals
        GrossSalary = _components
            .Where(c => c.ComponentType == ComponentType.Earning)
            .Sum(c => c.CalculatedAmount);
        TotalDeductions = _components
            .Where(c => c.ComponentType == ComponentType.Deduction)
            .Sum(c => c.CalculatedAmount);
        NetSalary = GrossSalary - TotalDeductions;

        SetUpdatedAt();

        AddDomainEvent(new PayrollEntryComponentOverriddenEvent(
            componentId, Id, PayrollRunId, CompanyId,
            component.ComponentCode, oldAmount, newAmount, overriddenBy, reason));

        return component;
    }

    // ── Status transitions ────────────────────────────────────────────────────

    /// <summary>Marks this entry as Disputed when a DisputeTicket is raised against it.</summary>
    public void MarkDisputed()
    {
        if (Status == PayrollEntryStatus.Locked)
            throw new PayrollFrozenException("Cannot dispute a locked payroll entry.");

        Status = PayrollEntryStatus.Disputed;
        SetUpdatedAt();
    }

    /// <summary>Marks this entry as Revised when a Correction run generates a new entry for this employee.</summary>
    public void MarkRevised()
    {
        Status = PayrollEntryStatus.Revised;
        SetUpdatedAt();
    }

    /// <summary>Locks the entry after disbursement. No further changes allowed.</summary>
    public void Lock()
    {
        Status = PayrollEntryStatus.Locked;
        SetUpdatedAt();
    }
}

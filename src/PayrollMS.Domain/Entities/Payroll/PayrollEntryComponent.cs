using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Payroll;

/// <summary>
/// A single salary component line item within a PayrollEntry.
/// All fields are immutable snapshots taken at calculation time — PRD §2.4, §15.2
///
/// Hard rule (AGENTS.md): Never edit once the parent PayrollRun.Status is past GENERATED.
/// The PayrollEntry.OverrideComponent method enforces this guard.
/// </summary>
public sealed class PayrollEntryComponent : BaseEntity<PayrollEntryComponentId>
{
    private PayrollEntryComponent() { }

    public PayrollEntryId PayrollEntryId { get; private set; }
    public CompanyId CompanyId { get; private set; }
    public SalaryComponentId SalaryComponentId { get; private set; }

    /// <summary>Snapshot of component code at calculation time — PRD §2.4</summary>
    public string ComponentCode { get; private set; } = null!;

    /// <summary>Snapshot of component name at calculation time.</summary>
    public string ComponentName { get; private set; } = null!;

    /// <summary>Snapshot of component type (Earning/Deduction) — drives gross/net aggregation.</summary>
    public ComponentType ComponentType { get; private set; }

    /// <summary>
    /// Snapshot of the exact NCalc formula evaluated to produce CalculatedAmount.
    /// Preserved for dispute investigation and audit — PRD §2.4, erd-explained §PayrollEntryComponent
    /// </summary>
    public string FormulaUsed { get; private set; } = null!;

    /// <summary>Result of formula evaluation. Stored as NUMERIC(12,2) in DB — PRD §27.3</summary>
    public decimal CalculatedAmount { get; private set; }

    /// <summary>
    /// True when HR manually edited this amount before approval — PRD §15.7
    /// Explicitly audited — AGENTS.md hard rule, PRD §24.3
    /// </summary>
    public bool IsManualOverride { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static PayrollEntryComponent Create(
        PayrollEntryId payrollEntryId,
        CompanyId companyId,
        SalaryComponentId salaryComponentId,
        string componentCode,
        string componentName,
        ComponentType componentType,
        string formulaUsed,
        decimal calculatedAmount)
    {
        return new PayrollEntryComponent
        {
            Id = PayrollEntryComponentId.New(),
            PayrollEntryId = payrollEntryId,
            CompanyId = companyId,
            SalaryComponentId = salaryComponentId,
            ComponentCode = componentCode.Trim(),
            ComponentName = componentName.Trim(),
            ComponentType = componentType,
            FormulaUsed = formulaUsed,
            CalculatedAmount = calculatedAmount,
            IsManualOverride = false
        };
    }

    // ── Manual override (called by PayrollEntry.OverrideComponent) ────────────

    /// <summary>
    /// Updates the calculated amount with a manual HR override.
    /// Guard is enforced at the PayrollEntry level, not here.
    /// PRD §15.7
    /// </summary>
    internal void ApplyOverride(decimal newAmount)
    {
        if (newAmount < 0)
            throw new BusinessRuleViolationException("InvalidOverrideAmount",
                "Override amount cannot be negative.");

        CalculatedAmount = newAmount;
        IsManualOverride = true;
        SetUpdatedAt();
    }
}

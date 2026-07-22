using PayrollMS.Domain.Common;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Payroll;

/// <summary>
/// Monthly leave summary synced from the HR or Leave Management system.
/// PayrollMS consumes leave summaries only — it never owns leave approval workflows. PRD §14.1
///
/// Leave type treatment maps to payroll impact:
///   Paid Leave     → counts as WorkingDays (no deduction)
///   Unpaid Leave   → deducted as AbsentDays (if opted in)
///   Medical/Casual → company-configurable (treated as paid in most tenants)
///   Half Days      → 50% daily rate deducted (if opted in)
///
/// PRD §14.3
/// </summary>
public sealed class LeaveSummary : BaseEntity<LeaveSummaryId>
{
    private LeaveSummary() { }

    public CompanyId CompanyId { get; private set; }

    /// <summary>Matches EmployeePayrollProfile.ExternalEmployeeId.</summary>
    public string ExternalEmployeeId { get; private set; } = null!;

    public int PeriodYear { get; private set; }
    public int PeriodMonth { get; private set; }

    /// <summary>
    /// Decimal to support partial-day leaves.
    /// PRD §14.2: "decimal to handle partial days"
    /// </summary>
    public decimal PaidLeaveDays { get; private set; }
    public decimal UnpaidLeaveDays { get; private set; }
    public decimal MedicalLeaveDays { get; private set; }
    public decimal CasualLeaveDays { get; private set; }
    public decimal HalfDays { get; private set; }

    public DateTimeOffset SyncedAt { get; private set; }

    /// <summary>
    /// Unique key per company+employee+period.
    /// Duplicate syncs with the same key are silently accepted — PRD §13.4 (same rule applies to leave).
    /// </summary>
    public string IdempotencyKey { get; private set; } = null!;

    // ── Factory ───────────────────────────────────────────────────────────────

    public static LeaveSummary Create(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        decimal paidLeaveDays,
        decimal unpaidLeaveDays,
        decimal medicalLeaveDays,
        decimal casualLeaveDays,
        decimal halfDays,
        string idempotencyKey)
    {
        Validate(companyId, externalEmployeeId, periodYear, periodMonth,
            paidLeaveDays, unpaidLeaveDays, medicalLeaveDays, casualLeaveDays, halfDays, idempotencyKey);

        var summary = new LeaveSummary
        {
            Id = LeaveSummaryId.New(),
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId.Trim(),
            PeriodYear = periodYear,
            PeriodMonth = periodMonth,
            PaidLeaveDays = paidLeaveDays,
            UnpaidLeaveDays = unpaidLeaveDays,
            MedicalLeaveDays = medicalLeaveDays,
            CasualLeaveDays = casualLeaveDays,
            HalfDays = halfDays,
            SyncedAt = DateTimeOffset.UtcNow,
            IdempotencyKey = idempotencyKey.Trim()
        };

        summary.AddDomainEvent(new LeaveSyncedEvent(
            summary.Id, companyId, externalEmployeeId, periodYear, periodMonth, IsUpdate: false));

        return summary;
    }

    // ── Update (re-sync while period is open) ────────────────────────────────

    public void Update(
        decimal paidLeaveDays,
        decimal unpaidLeaveDays,
        decimal medicalLeaveDays,
        decimal casualLeaveDays,
        decimal halfDays)
    {
        if (paidLeaveDays < 0) throw new BusinessRuleViolationException("InvalidPaidLeave", "PaidLeaveDays cannot be negative.");
        if (unpaidLeaveDays < 0) throw new BusinessRuleViolationException("InvalidUnpaidLeave", "UnpaidLeaveDays cannot be negative.");
        if (medicalLeaveDays < 0) throw new BusinessRuleViolationException("InvalidMedicalLeave", "MedicalLeaveDays cannot be negative.");
        if (casualLeaveDays < 0) throw new BusinessRuleViolationException("InvalidCasualLeave", "CasualLeaveDays cannot be negative.");
        if (halfDays < 0) throw new BusinessRuleViolationException("InvalidHalfDays", "HalfDays cannot be negative.");

        PaidLeaveDays = paidLeaveDays;
        UnpaidLeaveDays = unpaidLeaveDays;
        MedicalLeaveDays = medicalLeaveDays;
        CasualLeaveDays = casualLeaveDays;
        HalfDays = halfDays;
        SyncedAt = DateTimeOffset.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new LeaveSyncedEvent(
            Id, CompanyId, ExternalEmployeeId, PeriodYear, PeriodMonth, IsUpdate: true));
    }

    // ── Private validation ────────────────────────────────────────────────────

    private static void Validate(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        decimal paidLeaveDays,
        decimal unpaidLeaveDays,
        decimal medicalLeaveDays,
        decimal casualLeaveDays,
        decimal halfDays,
        string idempotencyKey)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "LeaveSummary must belong to a company.");

        if (string.IsNullOrWhiteSpace(externalEmployeeId))
            throw new BusinessRuleViolationException("ExternalIdRequired", "ExternalEmployeeId is required.");

        if (periodMonth is < 1 or > 12)
            throw new BusinessRuleViolationException("InvalidMonth", "PeriodMonth must be between 1 and 12.");

        if (periodYear < 2000 || periodYear > 2100)
            throw new BusinessRuleViolationException("InvalidYear", "PeriodYear is out of the acceptable range.");

        if (paidLeaveDays < 0) throw new BusinessRuleViolationException("InvalidPaidLeave", "PaidLeaveDays cannot be negative.");
        if (unpaidLeaveDays < 0) throw new BusinessRuleViolationException("InvalidUnpaidLeave", "UnpaidLeaveDays cannot be negative.");
        if (medicalLeaveDays < 0) throw new BusinessRuleViolationException("InvalidMedicalLeave", "MedicalLeaveDays cannot be negative.");
        if (casualLeaveDays < 0) throw new BusinessRuleViolationException("InvalidCasualLeave", "CasualLeaveDays cannot be negative.");
        if (halfDays < 0) throw new BusinessRuleViolationException("InvalidHalfDays", "HalfDays cannot be negative.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new BusinessRuleViolationException("IdempotencyKeyRequired", "IdempotencyKey is required for leave sync.");
    }
}

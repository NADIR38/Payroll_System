using PayrollMS.Domain.Common;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Payroll;

/// <summary>
/// Monthly attendance summary synced from an external ERP system.
/// PayrollMS never owns raw biometric data — only the monthly rollup. PRD §13.1
///
/// Idempotency: duplicate syncs with the same IdempotencyKey are silently accepted — PRD §13.4
/// Freeze rule: sync is rejected if the PayrollCalendar for the period is Frozen/Closed — PRD §13.4
/// </summary>
public sealed class AttendanceSummary : BaseEntity<AttendanceSummaryId>
{
    private AttendanceSummary() { }

    public CompanyId CompanyId { get; private set; }

    /// <summary>Matches EmployeePayrollProfile.ExternalEmployeeId.</summary>
    public string ExternalEmployeeId { get; private set; } = null!;

    public int PeriodYear { get; private set; }
    public int PeriodMonth { get; private set; }

    /// <summary>Actual days the employee was present and worked.</summary>
    public int WorkingDays { get; private set; }

    public int AbsentDays { get; private set; }
    public int LateDays { get; private set; }

    /// <summary>Total accumulated late minutes across all late arrivals in the period.</summary>
    public int LateMinutes { get; private set; }

    public decimal OvertimeHours { get; private set; }
    public int HalfDays { get; private set; }

    /// <summary>Public holidays in this period (not counted as absent).</summary>
    public int Holidays { get; private set; }

    public int Weekends { get; private set; }
    public DateTimeOffset SyncedAt { get; private set; }

    /// <summary>E.g. "SchoolERP", "HospitalERP" — for tracing.</summary>
    public string SourceSystem { get; private set; } = null!;

    /// <summary>
    /// Unique key per company+employee+period, supplied by the calling ERP.
    /// Format convention: "{CompanyCode}-{ExternalEmployeeId}-{Year}-{Month:D2}"
    /// PRD §13.2
    /// </summary>
    public string IdempotencyKey { get; private set; } = null!;

    // ── Factory ───────────────────────────────────────────────────────────────

    public static AttendanceSummary Create(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        int workingDays,
        int absentDays,
        int lateDays,
        int lateMinutes,
        decimal overtimeHours,
        int halfDays,
        int holidays,
        int weekends,
        string idempotencyKey,
        string sourceSystem = "Manual")
    {
        Validate(companyId, externalEmployeeId, periodYear, periodMonth,
            workingDays, absentDays, lateDays, lateMinutes, overtimeHours, halfDays, idempotencyKey);

        var summary = new AttendanceSummary
        {
            Id = AttendanceSummaryId.New(),
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId.Trim(),
            PeriodYear = periodYear,
            PeriodMonth = periodMonth,
            WorkingDays = workingDays,
            AbsentDays = absentDays,
            LateDays = lateDays,
            LateMinutes = lateMinutes,
            OvertimeHours = overtimeHours,
            HalfDays = halfDays,
            Holidays = holidays,
            Weekends = weekends,
            SyncedAt = DateTimeOffset.UtcNow,
            SourceSystem = sourceSystem.Trim(),
            IdempotencyKey = idempotencyKey.Trim()
        };

        summary.AddDomainEvent(new AttendanceSyncedEvent(
            summary.Id, companyId, externalEmployeeId, periodYear, periodMonth, IsUpdate: false));

        return summary;
    }

    // ── Update (re-sync while period is still in DRAFT) ───────────────────────

    /// <summary>
    /// Updates an existing record when a re-sync arrives for the same idempotency key.
    /// Only allowed while the PayrollCalendar is not Frozen — caller must enforce this.
    /// PRD §13.4
    /// </summary>
    public void Update(
        int workingDays,
        int absentDays,
        int lateDays,
        int lateMinutes,
        decimal overtimeHours,
        int halfDays,
        int holidays,
        int weekends,
        string sourceSystem)
    {
        if (workingDays < 0) throw new BusinessRuleViolationException("InvalidWorkingDays", "WorkingDays cannot be negative.");
        if (absentDays < 0) throw new BusinessRuleViolationException("InvalidAbsentDays", "AbsentDays cannot be negative.");
        if (lateDays < 0) throw new BusinessRuleViolationException("InvalidLateDays", "LateDays cannot be negative.");
        if (lateMinutes < 0) throw new BusinessRuleViolationException("InvalidLateMinutes", "LateMinutes cannot be negative.");
        if (overtimeHours < 0) throw new BusinessRuleViolationException("InvalidOvertimeHours", "OvertimeHours cannot be negative.");
        if (halfDays < 0) throw new BusinessRuleViolationException("InvalidHalfDays", "HalfDays cannot be negative.");

        WorkingDays = workingDays;
        AbsentDays = absentDays;
        LateDays = lateDays;
        LateMinutes = lateMinutes;
        OvertimeHours = overtimeHours;
        HalfDays = halfDays;
        Holidays = holidays;
        Weekends = weekends;
        SyncedAt = DateTimeOffset.UtcNow;
        SourceSystem = sourceSystem.Trim();
        SetUpdatedAt();

        AddDomainEvent(new AttendanceSyncedEvent(
            Id, CompanyId, ExternalEmployeeId, PeriodYear, PeriodMonth, IsUpdate: true));
    }

    // ── Private validation ────────────────────────────────────────────────────

    private static void Validate(
        CompanyId companyId,
        string externalEmployeeId,
        int periodYear,
        int periodMonth,
        int workingDays,
        int absentDays,
        int lateDays,
        int lateMinutes,
        decimal overtimeHours,
        int halfDays,
        string idempotencyKey)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "AttendanceSummary must belong to a company.");

        if (string.IsNullOrWhiteSpace(externalEmployeeId))
            throw new BusinessRuleViolationException("ExternalIdRequired", "ExternalEmployeeId is required.");

        if (periodMonth is < 1 or > 12)
            throw new BusinessRuleViolationException("InvalidMonth", "PeriodMonth must be between 1 and 12.");

        if (periodYear < 2000 || periodYear > 2100)
            throw new BusinessRuleViolationException("InvalidYear", "PeriodYear is out of the acceptable range.");

        if (workingDays < 0) throw new BusinessRuleViolationException("InvalidWorkingDays", "WorkingDays cannot be negative.");
        if (absentDays < 0) throw new BusinessRuleViolationException("InvalidAbsentDays", "AbsentDays cannot be negative.");
        if (lateDays < 0) throw new BusinessRuleViolationException("InvalidLateDays", "LateDays cannot be negative.");
        if (lateMinutes < 0) throw new BusinessRuleViolationException("InvalidLateMinutes", "LateMinutes cannot be negative.");
        if (overtimeHours < 0) throw new BusinessRuleViolationException("InvalidOvertimeHours", "OvertimeHours cannot be negative.");
        if (halfDays < 0) throw new BusinessRuleViolationException("InvalidHalfDays", "HalfDays cannot be negative.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new BusinessRuleViolationException("IdempotencyKeyRequired", "IdempotencyKey is required for attendance sync.");
    }
}

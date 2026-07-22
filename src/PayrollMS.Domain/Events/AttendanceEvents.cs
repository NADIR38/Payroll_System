using PayrollMS.Domain.Common;

namespace PayrollMS.Domain.Events;

/// <summary>
/// Fired when an AttendanceSummary is synced for an employee.
/// PRD §13.3 — POST /api/v1/attendance/sync
/// </summary>
public sealed record AttendanceSyncedEvent(
    AttendanceSummaryId AttendanceSummaryId,
    CompanyId CompanyId,
    string ExternalEmployeeId,
    int PeriodYear,
    int PeriodMonth,
    bool IsUpdate) : IDomainEvent;

/// <summary>
/// Fired when a LeaveSummary is synced for an employee.
/// PRD §14.2 — POST /api/v1/leave/sync
/// </summary>
public sealed record LeaveSyncedEvent(
    LeaveSummaryId LeaveSummaryId,
    CompanyId CompanyId,
    string ExternalEmployeeId,
    int PeriodYear,
    int PeriodMonth,
    bool IsUpdate) : IDomainEvent;

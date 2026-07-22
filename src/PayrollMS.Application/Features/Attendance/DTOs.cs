namespace PayrollMS.Application.Features.Attendance;

public sealed record AttendanceSummaryResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalEmployeeId,
    int PeriodYear,
    int PeriodMonth,
    int WorkingDays,
    int AbsentDays,
    int LateDays,
    int LateMinutes,
    decimal OvertimeHours,
    int HalfDays,
    int Holidays,
    int Weekends,
    DateTimeOffset SyncedAt,
    string SourceSystem,
    string IdempotencyKey);

public sealed record LeaveSummaryResponse(
    Guid Id,
    Guid CompanyId,
    string ExternalEmployeeId,
    int PeriodYear,
    int PeriodMonth,
    decimal PaidLeaveDays,
    decimal UnpaidLeaveDays,
    decimal MedicalLeaveDays,
    decimal CasualLeaveDays,
    decimal HalfDays,
    DateTimeOffset SyncedAt,
    string IdempotencyKey);

public sealed record BulkSyncItemResult(
    string ExternalEmployeeId,
    bool Success,
    string? Error);

public sealed record BulkSyncResult(
    int TotalProcessed,
    int TotalSucceeded,
    int TotalFailed,
    IReadOnlyList<BulkSyncItemResult> Items);

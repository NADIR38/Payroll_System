using MediatR;

namespace PayrollMS.Application.Features.Attendance.Commands.SyncAttendance;

public sealed record SyncAttendanceCommand(
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
    string IdempotencyKey,
    string SourceSystem = "Manual") : IRequest<AttendanceSummaryResponse>;

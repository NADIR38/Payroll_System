using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Attendance.Commands.SyncAttendance;

public sealed class SyncAttendanceCommandHandler : IRequestHandler<SyncAttendanceCommand, AttendanceSummaryResponse>
{
    private readonly IAttendanceSummaryRepository _attendanceRepo;
    private readonly IEmployeePayrollProfileRepository _employeeRepo;
    private readonly IPayrollCalendarRepository _calendarRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SyncAttendanceCommandHandler(
        IAttendanceSummaryRepository attendanceRepo,
        IEmployeePayrollProfileRepository employeeRepo,
        IPayrollCalendarRepository calendarRepo,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _calendarRepo = calendarRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttendanceSummaryResponse> Handle(SyncAttendanceCommand request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        // 1. Idempotency check — PRD §13.4: duplicate syncs with same key are silently accepted
        var existing = await _attendanceRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null)
        {
            return MapToResponse(existing);
        }

        // 2. Validate calendar status (reject if Frozen or Closed) — PRD §13.4
        var calendar = await _calendarRepo.GetByMonthYearAsync(companyId, request.PeriodMonth, request.PeriodYear, cancellationToken);
        if (calendar != null && calendar.Status != Domain.Enums.PayrollCalendarStatus.Open)
        {
            throw new BusinessRuleViolationException("PayrollPeriodFrozen",
                $"Cannot sync attendance for period {request.PeriodYear}-{request.PeriodMonth:D2} because the payroll calendar status is '{calendar.Status}'.");
        }

        // 3. Verify employee exists in tenant
        var employee = await _employeeRepo.GetByExternalIdAsync(companyId, request.ExternalEmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException($"Employee with external ID '{request.ExternalEmployeeId}' not found in company.");
        }

        // 4. Create or Update by employee & period
        var existingForPeriod = await _attendanceRepo.GetByEmployeeAndPeriodAsync(
            companyId, request.ExternalEmployeeId, request.PeriodYear, request.PeriodMonth, cancellationToken);

        AttendanceSummary summary;
        if (existingForPeriod != null)
        {
            existingForPeriod.Update(
                request.WorkingDays, request.AbsentDays, request.LateDays, request.LateMinutes,
                request.OvertimeHours, request.HalfDays, request.Holidays, request.Weekends, request.SourceSystem);
            summary = existingForPeriod;
            _attendanceRepo.Update(summary);
        }
        else
        {
            summary = AttendanceSummary.Create(
                companyId, request.ExternalEmployeeId, request.PeriodYear, request.PeriodMonth,
                request.WorkingDays, request.AbsentDays, request.LateDays, request.LateMinutes,
                request.OvertimeHours, request.HalfDays, request.Holidays, request.Weekends,
                request.IdempotencyKey, request.SourceSystem);
            await _attendanceRepo.AddAsync(summary, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(summary);
    }

    private static AttendanceSummaryResponse MapToResponse(AttendanceSummary summary) =>
        new(
            summary.Id.Value,
            summary.CompanyId.Value,
            summary.ExternalEmployeeId,
            summary.PeriodYear,
            summary.PeriodMonth,
            summary.WorkingDays,
            summary.AbsentDays,
            summary.LateDays,
            summary.LateMinutes,
            summary.OvertimeHours,
            summary.HalfDays,
            summary.Holidays,
            summary.Weekends,
            summary.SyncedAt,
            summary.SourceSystem,
            summary.IdempotencyKey);
}

using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Attendance.Commands.SyncLeave;

public sealed record SyncLeaveCommand(
    Guid CompanyId,
    string ExternalEmployeeId,
    int PeriodYear,
    int PeriodMonth,
    decimal PaidLeaveDays,
    decimal UnpaidLeaveDays,
    decimal MedicalLeaveDays,
    decimal CasualLeaveDays,
    decimal HalfDays,
    string IdempotencyKey) : IRequest<LeaveSummaryResponse>;

public sealed class SyncLeaveCommandHandler : IRequestHandler<SyncLeaveCommand, LeaveSummaryResponse>
{
    private readonly ILeaveSummaryRepository _leaveRepo;
    private readonly IEmployeePayrollProfileRepository _employeeRepo;
    private readonly IPayrollCalendarRepository _calendarRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SyncLeaveCommandHandler(
        ILeaveSummaryRepository leaveRepo,
        IEmployeePayrollProfileRepository employeeRepo,
        IPayrollCalendarRepository calendarRepo,
        IUnitOfWork unitOfWork)
    {
        _leaveRepo = leaveRepo;
        _employeeRepo = employeeRepo;
        _calendarRepo = calendarRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveSummaryResponse> Handle(SyncLeaveCommand request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        var existing = await _leaveRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null)
        {
            return MapToResponse(existing);
        }

        var calendar = await _calendarRepo.GetByMonthYearAsync(companyId, request.PeriodMonth, request.PeriodYear, cancellationToken);
        if (calendar != null && calendar.Status != Domain.Enums.PayrollCalendarStatus.Open)
        {
            throw new BusinessRuleViolationException("PayrollPeriodFrozen",
                $"Cannot sync leave for period {request.PeriodYear}-{request.PeriodMonth:D2} because payroll calendar status is '{calendar.Status}'.");
        }

        var employee = await _employeeRepo.GetByExternalIdAsync(companyId, request.ExternalEmployeeId, cancellationToken);
        if (employee == null)
        {
            throw new NotFoundException($"Employee with external ID '{request.ExternalEmployeeId}' not found in company.");
        }

        var existingForPeriod = await _leaveRepo.GetByEmployeeAndPeriodAsync(
            companyId, request.ExternalEmployeeId, request.PeriodYear, request.PeriodMonth, cancellationToken);

        LeaveSummary summary;
        if (existingForPeriod != null)
        {
            existingForPeriod.Update(
                request.PaidLeaveDays, request.UnpaidLeaveDays, request.MedicalLeaveDays,
                request.CasualLeaveDays, request.HalfDays);
            summary = existingForPeriod;
            _leaveRepo.Update(summary);
        }
        else
        {
            summary = LeaveSummary.Create(
                companyId, request.ExternalEmployeeId, request.PeriodYear, request.PeriodMonth,
                request.PaidLeaveDays, request.UnpaidLeaveDays, request.MedicalLeaveDays,
                request.CasualLeaveDays, request.HalfDays, request.IdempotencyKey);
            await _leaveRepo.AddAsync(summary, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToResponse(summary);
    }

    private static LeaveSummaryResponse MapToResponse(LeaveSummary summary) =>
        new(
            summary.Id.Value,
            summary.CompanyId.Value,
            summary.ExternalEmployeeId,
            summary.PeriodYear,
            summary.PeriodMonth,
            summary.PaidLeaveDays,
            summary.UnpaidLeaveDays,
            summary.MedicalLeaveDays,
            summary.CasualLeaveDays,
            summary.HalfDays,
            summary.SyncedAt,
            summary.IdempotencyKey);
}

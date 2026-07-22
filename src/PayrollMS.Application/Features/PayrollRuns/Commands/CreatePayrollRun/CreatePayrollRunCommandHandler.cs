using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.CreatePayrollRun;

public sealed class CreatePayrollRunCommandHandler : IRequestHandler<CreatePayrollRunCommand, PayrollRunCreatedResult>
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollCalendarRepository _calendarRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePayrollRunCommandHandler(
        IPayrollRunRepository payrollRunRepo,
        IPayrollCalendarRepository calendarRepo,
        IUnitOfWork unitOfWork)
    {
        _payrollRunRepo = payrollRunRepo;
        _calendarRepo = calendarRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollRunCreatedResult> Handle(CreatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);
        var finYearId = new FinancialYearId(request.FinancialYearId);

        // 1. Check if PayrollCalendar for period is frozen or closed — PRD §6.4
        var calendar = await _calendarRepo.GetByMonthYearAsync(companyId, request.PeriodMonth, request.PeriodYear, cancellationToken);
        if (calendar != null && calendar.Status != PayrollCalendarStatus.Open)
        {
            throw new BusinessRuleViolationException("PayrollPeriodFrozen",
                $"Cannot create payroll run for period {request.PeriodYear}-{request.PeriodMonth:D2} because payroll calendar status is '{calendar.Status}'.");
        }

        // 2. Validate no existing Regular run for same period — PRD §15.4
        if (request.RunType == PayrollRunType.Regular)
        {
            var exists = await _payrollRunRepo.ExistsForPeriodAsync(
                companyId, request.PeriodYear, request.PeriodMonth, PayrollRunType.Regular, cancellationToken);

            if (exists)
            {
                throw new BusinessRuleViolationException("PayrollAlreadyExists",
                    $"A Regular payroll run for period {request.PeriodYear}-{request.PeriodMonth:D2} already exists for this company.");
            }
        }

        // 3. Create run entity
        BranchId? branchId = request.FilterBranchId.HasValue ? new BranchId(request.FilterBranchId.Value) : null;
        DepartmentId? deptId = request.FilterDepartmentId.HasValue ? new DepartmentId(request.FilterDepartmentId.Value) : null;

        var run = PayrollRun.Create(
            companyId,
            finYearId,
            request.PeriodYear,
            request.PeriodMonth,
            request.RunType,
            request.CreatedBy,
            branchId,
            deptId,
            request.Remarks);

        await _payrollRunRepo.AddAsync(run, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // NOTE: Hangfire worker trigger will be wired in Infrastructure or worker launcher service
        return new PayrollRunCreatedResult(
            run.Id.Value,
            run.Status.ToString(),
            "Payroll generation queued. Poll status endpoint for progress.");
    }
}

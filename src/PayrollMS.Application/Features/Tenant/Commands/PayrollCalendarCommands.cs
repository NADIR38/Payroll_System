using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE PAYROLL CALENDAR
// ==========================================

public sealed record CreatePayrollCalendarCommand(
    Guid CompanyId,
    Guid FinancialYearId,
    int Month,
    int Year,
    DateOnly PayrollFreezeDate,
    DateOnly PaymentDate,
    int WorkingDays,
    string? Holidays) : IRequest<Guid>;

public class CreatePayrollCalendarCommandValidator : AbstractValidator<CreatePayrollCalendarCommand>
{
    public CreatePayrollCalendarCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FinancialYearId).NotEmpty();
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).GreaterThan(2000);
        RuleFor(x => x.PayrollFreezeDate).NotEmpty();
        RuleFor(x => x.PaymentDate).NotEmpty().GreaterThanOrEqualTo(x => x.PayrollFreezeDate)
            .WithMessage("Payment date must be on or after the freeze date.");
        RuleFor(x => x.WorkingDays).InclusiveBetween(1, 31);
    }
}

public class CreatePayrollCalendarCommandHandler : IRequestHandler<CreatePayrollCalendarCommand, Guid>
{
    private readonly IPayrollCalendarRepository _payrollCalendarRepository;
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePayrollCalendarCommandHandler(
        IPayrollCalendarRepository payrollCalendarRepository,
        IFinancialYearRepository financialYearRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _payrollCalendarRepository = payrollCalendarRepository;
        _financialYearRepository = financialYearRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePayrollCalendarCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        var financialYearId = new FinancialYearId(request.FinancialYearId);
        var financialYear = await _financialYearRepository.GetByIdForUpdateAsync(financialYearId, cancellationToken);
        if (financialYear == null)
        {
            throw new NotFoundException($"Financial Year with ID '{request.FinancialYearId}' was not found.");
        }

        var existing = await _payrollCalendarRepository.GetByMonthYearAsync(companyId, request.Month, request.Year, cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleViolationException("DUPLICATE_CALENDAR", $"Payroll calendar for {request.Month}/{request.Year} already exists.");
        }

        var calendar = PayrollCalendar.Create(
            companyId,
            financialYearId,
            request.Month,
            request.Year,
            request.PayrollFreezeDate,
            request.PaymentDate,
            request.WorkingDays,
            request.Holidays);

        await _payrollCalendarRepository.AddAsync(calendar, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return calendar.Id.Value;
    }
}

// ==========================================
// 2. FREEZE PAYROLL CALENDAR
// ==========================================

public sealed record FreezePayrollCalendarCommand(Guid Id) : IRequest;

public class FreezePayrollCalendarCommandHandler : IRequestHandler<FreezePayrollCalendarCommand>
{
    private readonly IPayrollCalendarRepository _payrollCalendarRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FreezePayrollCalendarCommandHandler(IPayrollCalendarRepository payrollCalendarRepository, IUnitOfWork unitOfWork)
    {
        _payrollCalendarRepository = payrollCalendarRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(FreezePayrollCalendarCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var calendarId = new PayrollCalendarId(request.Id);
        var calendar = await _payrollCalendarRepository.GetByIdForUpdateAsync(calendarId, cancellationToken);
        if (calendar == null)
        {
            throw new NotFoundException($"Payroll Calendar with ID '{request.Id}' was not found.");
        }

        calendar.Freeze();

        _payrollCalendarRepository.Update(calendar);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 3. CLOSE PAYROLL CALENDAR
// ==========================================

public sealed record ClosePayrollCalendarCommand(Guid Id) : IRequest;

public class ClosePayrollCalendarCommandHandler : IRequestHandler<ClosePayrollCalendarCommand>
{
    private readonly IPayrollCalendarRepository _payrollCalendarRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClosePayrollCalendarCommandHandler(IPayrollCalendarRepository payrollCalendarRepository, IUnitOfWork unitOfWork)
    {
        _payrollCalendarRepository = payrollCalendarRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ClosePayrollCalendarCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var calendarId = new PayrollCalendarId(request.Id);
        var calendar = await _payrollCalendarRepository.GetByIdForUpdateAsync(calendarId, cancellationToken);
        if (calendar == null)
        {
            throw new NotFoundException($"Payroll Calendar with ID '{request.Id}' was not found.");
        }

        calendar.Close();

        _payrollCalendarRepository.Update(calendar);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 4. REOPEN PAYROLL CALENDAR
// ==========================================

public sealed record ReopenPayrollCalendarCommand(Guid Id) : IRequest;

public class ReopenPayrollCalendarCommandHandler : IRequestHandler<ReopenPayrollCalendarCommand>
{
    private readonly IPayrollCalendarRepository _payrollCalendarRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenPayrollCalendarCommandHandler(IPayrollCalendarRepository payrollCalendarRepository, IUnitOfWork unitOfWork)
    {
        _payrollCalendarRepository = payrollCalendarRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReopenPayrollCalendarCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var calendarId = new PayrollCalendarId(request.Id);
        var calendar = await _payrollCalendarRepository.GetByIdForUpdateAsync(calendarId, cancellationToken);
        if (calendar == null)
        {
            throw new NotFoundException($"Payroll Calendar with ID '{request.Id}' was not found.");
        }

        calendar.Reopen();

        _payrollCalendarRepository.Update(calendar);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

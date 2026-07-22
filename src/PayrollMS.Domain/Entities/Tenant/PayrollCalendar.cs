using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class PayrollCalendar : BaseAuditableEntity<PayrollCalendarId>
{
    private PayrollCalendar()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public FinancialYearId FinancialYearId { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public DateOnly PayrollFreezeDate { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public int WorkingDays { get; private set; }
    public string? Holidays { get; private set; } // JSON format: [{date, name}]
    public PayrollCalendarStatus Status { get; private set; }

    public Company Company { get; private set; } = null!;
    public FinancialYear FinancialYear { get; private set; } = null!;

    public static PayrollCalendar Create(
        CompanyId companyId,
        FinancialYearId financialYearId,
        int month,
        int year,
        DateOnly payrollFreezeDate,
        DateOnly paymentDate,
        int workingDays,
        string? holidays = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "PayrollCalendar must belong to a company.");

        if (financialYearId == FinancialYearId.Empty)
            throw new BusinessRuleViolationException("FinancialYearRequired", "PayrollCalendar must belong to a financial year.");

        if (month < 1 || month > 12)
            throw new BusinessRuleViolationException("InvalidMonth", "Month must be between 1 and 12.");

        if (workingDays < 1 || workingDays > 31)
            throw new BusinessRuleViolationException("InvalidWorkingDays", "Working days must be valid for a month.");

        var calendar = new PayrollCalendar
        {
            Id = PayrollCalendarId.New(),
            CompanyId = companyId,
            FinancialYearId = financialYearId,
            Month = month,
            Year = year,
            PayrollFreezeDate = payrollFreezeDate,
            PaymentDate = paymentDate,
            WorkingDays = workingDays,
            Holidays = holidays,
            Status = PayrollCalendarStatus.Open
        };

        calendar.AddDomainEvent(new PayrollCalendarCreatedEvent(calendar.Id, calendar.CompanyId, calendar.Month, calendar.Year));

        return calendar;
    }

    public void Freeze()
    {
        if (Status == PayrollCalendarStatus.Closed)
            throw new BusinessRuleViolationException("InvalidState", "Cannot freeze an already closed payroll calendar.");

        Status = PayrollCalendarStatus.Frozen;
        SetUpdatedAt();
        AddDomainEvent(new PayrollCalendarFrozenEvent(Id));
    }

    public void Close()
    {
        if (Status != PayrollCalendarStatus.Frozen)
            throw new BusinessRuleViolationException("InvalidState", "Calendar must be frozen before it can be closed.");

        Status = PayrollCalendarStatus.Closed;
        SetUpdatedAt();
        AddDomainEvent(new PayrollCalendarClosedEvent(Id));
    }

    public void Reopen()
    {
        if (Status == PayrollCalendarStatus.Closed)
            throw new BusinessRuleViolationException("InvalidState", "Cannot reopen a closed payroll calendar.");

        Status = PayrollCalendarStatus.Open;
        SetUpdatedAt();
        AddDomainEvent(new PayrollCalendarReopenedEvent(Id));
    }
}

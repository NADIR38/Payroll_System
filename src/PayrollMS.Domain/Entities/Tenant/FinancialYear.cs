using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class FinancialYear : BaseAuditableEntity<FinancialYearId>
{
    private readonly List<PayrollCalendar> _payrollCalendars = [];

    private FinancialYear()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public string Label { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsCurrent { get; private set; }

    public Company Company { get; private set; } = null!;
    
    public IReadOnlyCollection<PayrollCalendar> PayrollCalendars => _payrollCalendars.AsReadOnly();

    public static FinancialYear Create(
        CompanyId companyId,
        string label,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "FinancialYear must belong to a company.");

        if (string.IsNullOrWhiteSpace(label))
            throw new BusinessRuleViolationException("LabelRequired", "FinancialYear label cannot be empty.");

        if (endDate <= startDate)
            throw new BusinessRuleViolationException("InvalidDates", "End date must be after start date.");

        var financialYear = new FinancialYear
        {
            Id = FinancialYearId.New(),
            CompanyId = companyId,
            Label = label.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsCurrent = false
        };

        financialYear.AddDomainEvent(new FinancialYearCreatedEvent(financialYear.Id, financialYear.CompanyId, financialYear.Label));

        return financialYear;
    }

    public void MarkAsCurrent()
    {
        if (IsCurrent)
            return;

        IsCurrent = true;
        SetUpdatedAt();
        AddDomainEvent(new FinancialYearMarkedCurrentEvent(Id, CompanyId));
    }

    public void RemoveCurrentStatus()
    {
        if (!IsCurrent)
            return;

        IsCurrent = false;
        SetUpdatedAt();
        AddDomainEvent(new FinancialYearCurrentRemovedEvent(Id, CompanyId));
    }
}

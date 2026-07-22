using PayrollMS.Domain.Common;
using PayrollMS.Domain.Common.ValueObjects;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class Company : BaseAuditableEntity<CompanyId>, IAggregateRoot
{
    private readonly List<Branch> _branches = [];
    private readonly List<Department> _departments = [];
    private readonly List<Designation> _designations = [];
    private readonly List<CostCenter> _costCenters = [];
    private readonly List<FinancialYear> _financialYears = [];

    private Company()
    {
    }

    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public Address? Address { get; private set; }
    public EmailAddress? ContactEmail { get; private set; }
    public PhoneNumber? ContactPhone { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<Designation> Designations => _designations.AsReadOnly();
    public IReadOnlyCollection<CostCenter> CostCenters => _costCenters.AsReadOnly();
    public IReadOnlyCollection<FinancialYear> FinancialYears => _financialYears.AsReadOnly();

    public static Company Create(
        string name,
        string code,
        EmailAddress? contactEmail = null,
        PhoneNumber? contactPhone = null,
        Address? address = null,
        string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Company name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Company code cannot be empty.");

        var company = new Company
        {
            Id = CompanyId.New(),
            Name = name.Trim(),
            Code = code.Trim(),
            LogoUrl = logoUrl,
            Address = address,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            IsActive = true
        };

        company.AddDomainEvent(new CompanyCreatedEvent(company.Id, company.Name, company.Code));

        return company;
    }

    public void UpdateDetails(
        string name,
        EmailAddress? contactEmail,
        PhoneNumber? contactPhone,
        Address? address,
        string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Company name cannot be empty.");

        Name = name.Trim();
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Address = address;
        LogoUrl = logoUrl;

        SetUpdatedAt();
        AddDomainEvent(new CompanyUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new CompanyActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new CompanyDeactivatedEvent(Id));
    }
}

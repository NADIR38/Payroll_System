using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class Designation : BaseAuditableEntity<DesignationId>
{
    private Designation()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Grade { get; private set; }
    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;

    public static Designation Create(
        CompanyId companyId,
        string name,
        string code,
        string? grade = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "Designation must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Designation name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Designation code cannot be empty.");

        var designation = new Designation
        {
            Id = DesignationId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim(),
            Grade = grade,
            IsActive = true
        };

        designation.AddDomainEvent(new DesignationCreatedEvent(designation.Id, designation.CompanyId, designation.Name, designation.Code, designation.Grade));

        return designation;
    }

    public void UpdateDetails(string name, string? grade)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Designation name cannot be empty.");

        Name = name.Trim();
        Grade = grade;

        SetUpdatedAt();
        AddDomainEvent(new DesignationUpdatedEvent(Id, Name, Grade));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new DesignationActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new DesignationDeactivatedEvent(Id));
    }
}

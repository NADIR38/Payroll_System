using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class Branch : BaseAuditableEntity<BranchId>
{
    private Branch()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;

    public static Branch Create(
        CompanyId companyId,
        string name,
        string code,
        string? address = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "Branch must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Branch name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Branch code cannot be empty.");

        var branch = new Branch
        {
            Id = BranchId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim(),
            Address = address,
            IsActive = true
        };

        branch.AddDomainEvent(new BranchCreatedEvent(branch.Id, branch.CompanyId, branch.Name, branch.Code));

        return branch;
    }

    public void UpdateDetails(string name, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Branch name cannot be empty.");

        Name = name.Trim();
        Address = address;

        SetUpdatedAt();
        AddDomainEvent(new BranchUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new BranchActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new BranchDeactivatedEvent(Id));
    }
}

using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class CostCenter : BaseAuditableEntity<CostCenterId>
{
    private CostCenter()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;

    public static CostCenter Create(
        CompanyId companyId,
        string name,
        string code)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "CostCenter must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "CostCenter name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "CostCenter code cannot be empty.");

        var costCenter = new CostCenter
        {
            Id = CostCenterId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim(),
            IsActive = true
        };

        costCenter.AddDomainEvent(new CostCenterCreatedEvent(costCenter.Id, costCenter.CompanyId, costCenter.Name, costCenter.Code));

        return costCenter;
    }

    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "CostCenter name cannot be empty.");

        Name = name.Trim();

        SetUpdatedAt();
        AddDomainEvent(new CostCenterUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new CostCenterActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new CostCenterDeactivatedEvent(Id));
    }
}

using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Events;

namespace PayrollMS.Domain.Entities.Tenant;

public sealed class Department : BaseAuditableEntity<DepartmentId>
{
    private Department()
    {
    }

    public CompanyId CompanyId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;
    public Branch? Branch { get; private set; }

    public static Department Create(
        CompanyId companyId,
        string name,
        string code,
        BranchId? branchId = null)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "Department must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Department name cannot be empty.");

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("CodeRequired", "Department code cannot be empty.");

        var department = new Department
        {
            Id = DepartmentId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim(),
            BranchId = branchId,
            IsActive = true
        };

        department.AddDomainEvent(new DepartmentCreatedEvent(department.Id, department.CompanyId, department.Name, department.Code));

        return department;
    }

    public void UpdateDetails(string name, BranchId? branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Department name cannot be empty.");

        Name = name.Trim();
        BranchId = branchId;

        SetUpdatedAt();
        AddDomainEvent(new DepartmentUpdatedEvent(Id, Name, BranchId));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new DepartmentActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new DepartmentDeactivatedEvent(Id));
    }
}

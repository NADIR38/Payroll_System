namespace PayrollMS.Domain.Common;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
{
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    public void SetCreatedBy(string createdBy)
    {
        CreatedBy = createdBy;
    }

    public void SetUpdatedBy(string updatedBy)
    {
        UpdatedBy = updatedBy;
    }
}

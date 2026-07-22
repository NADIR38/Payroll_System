using System.ComponentModel.DataAnnotations.Schema;

namespace PayrollMS.Domain.Common;

public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; init; } = default!;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; private set; }

    public uint Version { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void SetUpdatedAt()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetDeleted()
    {
        IsDeleted = true;
        SetUpdatedAt();
    }

    public void SetRestored()
    {
        IsDeleted = false;
        SetUpdatedAt();
    }

    public bool Equals(BaseEntity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseEntity<TId>);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !(left == right);
    }
}

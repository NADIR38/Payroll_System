using PayrollMS.Domain.Common;

namespace PayrollMS.Application.Interfaces;

public interface ICurrentTenant
{
    CompanyId? CompanyId { get; }
    string? UserId { get; }
}

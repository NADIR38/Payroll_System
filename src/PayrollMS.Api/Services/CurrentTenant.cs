using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;

namespace PayrollMS.Api.Services;

public class CurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CompanyId? CompanyId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // 1. Resolve company_id from request items (populated by auth middleware)
            if (httpContext.Items.TryGetValue("CompanyId", out var companyIdObj) && companyIdObj is CompanyId companyId)
            {
                return companyId;
            }

            // 2. Check headers "X-Tenant-Id" or "X-Company-Id"
            if ((httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) ||
                 httpContext.Request.Headers.TryGetValue("X-Company-Id", out tenantHeader)) && 
                Guid.TryParse(tenantHeader, out var guid) && guid != Guid.Empty)
            {
                return new CompanyId(guid);
            }

            return null;
        }
    }

    public string? UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.Name ?? "System";
        }
    }
}

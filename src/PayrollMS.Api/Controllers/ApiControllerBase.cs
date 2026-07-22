using MediatR;
using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Interfaces;

namespace PayrollMS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid GetCompanyId()
    {
        var currentTenant = HttpContext.RequestServices.GetRequiredService<ICurrentTenant>();
        if (currentTenant.CompanyId.HasValue && currentTenant.CompanyId.Value.Value != Guid.Empty)
        {
            return currentTenant.CompanyId.Value.Value;
        }

        // Fallback: Query first active company from database if no tenant header was supplied
        var dbContext = HttpContext.RequestServices.GetRequiredService<IAppDbContext>();
        var firstCompany = dbContext.Companies.FirstOrDefault(c => c.IsActive && !c.IsDeleted);
        return firstCompany?.Id.Value ?? Guid.Empty;
    }
}

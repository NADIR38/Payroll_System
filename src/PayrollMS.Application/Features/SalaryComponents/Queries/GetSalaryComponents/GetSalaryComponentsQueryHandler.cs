using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;

namespace PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponents;

public class GetSalaryComponentsQueryHandler : IRequestHandler<GetSalaryComponentsQuery, IReadOnlyList<SalaryComponentResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetSalaryComponentsQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SalaryComponentResponse>> Handle(GetSalaryComponentsQuery request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        var query = _dbContext.SalaryComponents
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId);

        if (request.IsActiveOnly.HasValue && request.IsActiveOnly.Value)
        {
            query = query.Where(c => c.IsActive);
        }

        var components = await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.TypeFilter))
        {
            components = components
                .Where(c => c.Type.ToString().Equals(request.TypeFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return components.Select(c => new SalaryComponentResponse(
            c.Id.Value,
            c.CompanyId.Value,
            c.Name,
            c.Code,
            c.Type.ToString(),
            c.CalculationMethod.ToString(),
            c.DefaultValue,
            c.IsTaxable,
            c.IsRecurring,
            c.IsOptional,
            c.IsActive,
            c.Description,
            c.CreatedAt)).ToList();
    }
}

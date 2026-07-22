using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponentById;

public class GetSalaryComponentByIdQueryHandler : IRequestHandler<GetSalaryComponentByIdQuery, SalaryComponentResponse>
{
    private readonly IAppDbContext _dbContext;

    public GetSalaryComponentByIdQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalaryComponentResponse> Handle(GetSalaryComponentByIdQuery request, CancellationToken cancellationToken)
    {
        var componentId = new SalaryComponentId(request.Id);
        var companyId = new CompanyId(request.CompanyId);

        var component = await _dbContext.SalaryComponents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == componentId && c.CompanyId == companyId, cancellationToken);

        if (component == null)
        {
            throw new NotFoundException($"Salary component with ID '{request.Id}' was not found for this company.");
        }

        return new SalaryComponentResponse(
            component.Id.Value,
            component.CompanyId.Value,
            component.Name,
            component.Code,
            component.Type.ToString(),
            component.CalculationMethod.ToString(),
            component.DefaultValue,
            component.IsTaxable,
            component.IsRecurring,
            component.IsOptional,
            component.IsActive,
            component.Description,
            component.CreatedAt);
    }
}

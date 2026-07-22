using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructureById;

public class GetSalaryStructureByIdQueryHandler : IRequestHandler<GetSalaryStructureByIdQuery, SalaryStructureResponse>
{
    private readonly IAppDbContext _dbContext;

    public GetSalaryStructureByIdQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalaryStructureResponse> Handle(GetSalaryStructureByIdQuery request, CancellationToken cancellationToken)
    {
        var structureId = new SalaryStructureId(request.Id);
        var companyId = new CompanyId(request.CompanyId);

        var structure = await _dbContext.SalaryStructures
            .AsNoTracking()
            .Include(s => s.Components.Where(c => c.IsActive))
            .FirstOrDefaultAsync(s => s.Id == structureId && s.CompanyId == companyId, cancellationToken);

        if (structure == null)
        {
            throw new NotFoundException($"Salary structure with ID '{request.Id}' was not found for this company.");
        }

        var componentIds = structure.Components.Select(c => c.SalaryComponentId).Distinct().ToList();
        var componentLookup = (await _dbContext.SalaryComponents
            .AsNoTracking()
            .Where(c => componentIds.Contains(c.Id))
            .ToListAsync(cancellationToken))
            .ToDictionary(c => c.Id);

        var structureComponentIds = structure.Components.Select(c => c.Id).ToList();
        var allowanceRules = (await _dbContext.AllowanceRules
            .AsNoTracking()
            .Where(a => structureComponentIds.Contains(a.SalaryStructureComponentId))
            .ToListAsync(cancellationToken))
            .ToDictionary(a => a.SalaryStructureComponentId);

        var deductionRules = (await _dbContext.DeductionRules
            .AsNoTracking()
            .Where(d => structureComponentIds.Contains(d.SalaryStructureComponentId))
            .ToListAsync(cancellationToken))
            .ToDictionary(d => d.SalaryStructureComponentId);

        return new SalaryStructureResponse(
            structure.Id.Value,
            structure.CompanyId.Value,
            structure.Name,
            structure.Code,
            structure.Description,
            structure.EffectiveFrom,
            structure.EffectiveTo,
            structure.IsActive,
            structure.CreatedAt,
            structure.Components.Select(c =>
            {
                componentLookup.TryGetValue(c.SalaryComponentId, out var comp);
                allowanceRules.TryGetValue(c.Id, out var aRule);
                deductionRules.TryGetValue(c.Id, out var dRule);

                return new SalaryStructureComponentResponse(
                    c.Id.Value,
                    c.SalaryStructureId.Value,
                    c.SalaryComponentId.Value,
                    comp?.Code ?? "UNKNOWN",
                    comp?.Name ?? "UNKNOWN",
                    comp?.Type.ToString() ?? "Earning",
                    comp?.CalculationMethod.ToString() ?? "Fixed",
                    c.FormulaExpression,
                    c.Sequence,
                    c.FixedAmount,
                    c.IsActive,
                    aRule != null ? new AllowanceRuleResponse(aRule.Id.Value, aRule.SalaryStructureComponentId.Value, aRule.CompanyId.Value, aRule.ApplicationMode.ToString(), aRule.ConditionExpression, aRule.Description, true) : null,
                    dRule != null ? new DeductionRuleResponse(dRule.Id.Value, dRule.SalaryStructureComponentId.Value, dRule.CompanyId.Value, dRule.DeductionType.ToString(), dRule.IsOptIn, dRule.GracePeriodMinutes, dRule.DeductionFormula, true) : null);
            }).OrderBy(c => c.Sequence).ToList());
    }
}

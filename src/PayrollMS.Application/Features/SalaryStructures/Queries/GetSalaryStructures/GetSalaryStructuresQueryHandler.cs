using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructures;

public class GetSalaryStructuresQueryHandler : IRequestHandler<GetSalaryStructuresQuery, IReadOnlyList<SalaryStructureResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetSalaryStructuresQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SalaryStructureResponse>> Handle(GetSalaryStructuresQuery request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        var query = _dbContext.SalaryStructures
            .AsNoTracking()
            .Include(s => s.Components.Where(c => c.IsActive))
            .Where(s => s.CompanyId == companyId);

        if (request.IsActiveOnly.HasValue && request.IsActiveOnly.Value)
        {
            query = query.Where(s => s.IsActive);
        }

        var structures = await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var componentIds = structures.SelectMany(s => s.Components).Select(c => c.SalaryComponentId).Distinct().ToList();
        var componentLookup = (await _dbContext.SalaryComponents
            .AsNoTracking()
            .Where(c => componentIds.Contains(c.Id))
            .ToListAsync(cancellationToken))
            .ToDictionary(c => c.Id);

        var structureComponentIds = structures.SelectMany(s => s.Components).Select(c => c.Id).ToList();
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

        return structures.Select(s => new SalaryStructureResponse(
            s.Id.Value,
            s.CompanyId.Value,
            s.Name,
            s.Code,
            s.Description,
            s.EffectiveFrom,
            s.EffectiveTo,
            s.IsActive,
            s.CreatedAt,
            s.Components.Select(c =>
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
            }).OrderBy(c => c.Sequence).ToList())).ToList();
    }
}

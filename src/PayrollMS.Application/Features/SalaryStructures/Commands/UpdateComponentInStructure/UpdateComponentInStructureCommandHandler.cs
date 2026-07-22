using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.UpdateComponentInStructure;

public class UpdateComponentInStructureCommandHandler : IRequestHandler<UpdateComponentInStructureCommand>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComponentInStructureCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IFormulaEvaluator formulaEvaluator,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _formulaEvaluator = formulaEvaluator;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateComponentInStructureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var structureId = new SalaryStructureId(request.SalaryStructureId);
        var companyId = new CompanyId(request.CompanyId);
        var structureComponentId = new SalaryStructureComponentId(request.StructureComponentId);

        var structure = await _salaryStructureRepository.GetByIdForUpdateAsync(structureId, cancellationToken);
        if (structure == null || structure.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary structure with ID '{request.SalaryStructureId}' was not found for this company.");
        }

        var component = structure.Components.FirstOrDefault(c => c.Id == structureComponentId && c.IsActive);
        if (component == null)
        {
            throw new NotFoundException($"Component with ID '{request.StructureComponentId}' not found in structure.");
        }

        if (!string.IsNullOrWhiteSpace(request.FormulaExpression) && request.FormulaExpression != "0")
        {
            var existingComponentIds = structure.Components
                .Where(c => c.IsActive && c.Sequence < request.Sequence && c.Id != structureComponentId)
                .Select(c => c.SalaryComponentId)
                .ToList();

            var availableComponentCodes = await _dbContext.SalaryComponents
                .AsNoTracking()
                .Where(c => existingComponentIds.Contains(c.Id))
                .Select(c => c.Code)
                .ToListAsync(cancellationToken);

            var availableVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BASE", "GROSS", "EARNINGS", "WORKING_DAYS", "WORKED_HOURS"
            };

            foreach (var code in availableComponentCodes)
            {
                availableVariables.Add(code);
            }

            var errors = _formulaEvaluator.Validate(request.FormulaExpression, availableVariables);
            if (errors.Count > 0)
            {
                throw new BusinessRuleViolationException(
                    "INVALID_FORMULA",
                    $"Formula validation failed: {string.Join("; ", errors)}");
            }
        }

        structure.UpdateComponent(
            structureComponentId,
            request.FormulaExpression,
            request.Sequence,
            request.FixedAmount);

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

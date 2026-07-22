using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.AddComponentToStructure;

public class AddComponentToStructureCommandHandler : IRequestHandler<AddComponentToStructureCommand, Guid>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public AddComponentToStructureCommandHandler(
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

    public async Task<Guid> Handle(AddComponentToStructureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var structureId = new SalaryStructureId(request.SalaryStructureId);
        var companyId = new CompanyId(request.CompanyId);
        var componentId = new SalaryComponentId(request.SalaryComponentId);

        // 1. Fetch Structure Aggregate Root
        var structure = await _salaryStructureRepository.GetByIdWithComponentsAsync(structureId, cancellationToken);
        if (structure == null || structure.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary structure with ID '{request.SalaryStructureId}' was not found for this company.");
        }

        // 2. Fetch target SalaryComponent to ensure existence in 1 query
        var targetComponent = await _dbContext.SalaryComponents
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == componentId && c.CompanyId == companyId, cancellationToken);

        if (targetComponent == null)
        {
            throw new NotFoundException($"Salary component with ID '{request.SalaryComponentId}' was not found for this company.");
        }

        // 3. Formula Engine Validation if formula expression is provided
        if (!string.IsNullOrWhiteSpace(request.FormulaExpression))
        {
            var existingComponentIds = structure.Components
                .Where(c => c.IsActive && c.Sequence < request.Sequence)
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
                    $"Formula validation failed for component '{targetComponent.Code}': {string.Join("; ", errors)}");
            }
        }

        // 4. Add component to Aggregate Root
        var structureComponent = structure.AddComponent(
            componentId,
            request.FormulaExpression,
            request.Sequence,
            request.FixedAmount);

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return structureComponent.Id.Value;
    }
}

using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetAllowanceRule;

public class SetAllowanceRuleCommandHandler : IRequestHandler<SetAllowanceRuleCommand, Guid>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IUnitOfWork _unitOfWork;

    public SetAllowanceRuleCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IFormulaEvaluator formulaEvaluator,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _formulaEvaluator = formulaEvaluator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SetAllowanceRuleCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var structureId = new SalaryStructureId(request.SalaryStructureId);
        var companyId = new CompanyId(request.CompanyId);
        var structureComponentId = new SalaryStructureComponentId(request.StructureComponentId);

        var structure = await _salaryStructureRepository.GetByIdWithComponentsAsync(structureId, cancellationToken);
        if (structure == null || structure.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary structure with ID '{request.SalaryStructureId}' was not found for this company.");
        }

        var appMode = Enum.Parse<AllowanceApplicationMode>(request.ApplicationMode, ignoreCase: true);

        // Validate condition expression if ConditionalExpression
        if (appMode == AllowanceApplicationMode.ConditionalExpression && !string.IsNullOrWhiteSpace(request.ConditionExpression))
        {
            var systemVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BASE", "GROSS", "EARNINGS", "WORKING_DAYS", "WORKED_HOURS"
            };

            var errors = _formulaEvaluator.Validate(request.ConditionExpression, systemVars);
            if (errors.Count > 0)
            {
                throw new BusinessRuleViolationException(
                    "INVALID_CONDITION_FORMULA",
                    $"Allowance condition formula validation failed: {string.Join("; ", errors)}");
            }
        }

        var rule = structure.SetAllowanceRule(
            structureComponentId,
            appMode,
            request.ConditionExpression,
            request.Description);

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return rule.Id.Value;
    }
}

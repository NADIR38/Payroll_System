using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetDeductionRule;

public class SetDeductionRuleCommandHandler : IRequestHandler<SetDeductionRuleCommand, Guid>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IFormulaEvaluator _formulaEvaluator;
    private readonly IUnitOfWork _unitOfWork;

    public SetDeductionRuleCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IFormulaEvaluator formulaEvaluator,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _formulaEvaluator = formulaEvaluator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SetDeductionRuleCommand request, CancellationToken cancellationToken)
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

        var dType = Enum.Parse<DeductionType>(request.DeductionType, ignoreCase: true);

        // Validate deduction formula if provided
        if (!string.IsNullOrWhiteSpace(request.DeductionFormula))
        {
            var systemVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BASE", "GROSS", "EARNINGS", "WORKING_DAYS", "WORKED_HOURS", "LATE_MINUTES", "ABSENT_DAYS"
            };

            var errors = _formulaEvaluator.Validate(request.DeductionFormula, systemVars);
            if (errors.Count > 0)
            {
                throw new BusinessRuleViolationException(
                    "INVALID_DEDUCTION_FORMULA",
                    $"Deduction formula validation failed: {string.Join("; ", errors)}");
            }
        }

        var rule = structure.SetDeductionRule(
            structureComponentId,
            dType,
            request.IsOptIn,
            request.GracePeriodMinutes,
            request.DeductionFormula);

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return rule.Id.Value;
    }
}

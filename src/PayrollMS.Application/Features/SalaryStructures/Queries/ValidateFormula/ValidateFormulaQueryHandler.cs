using MediatR;
using PayrollMS.Domain.Interfaces.Services;

namespace PayrollMS.Application.Features.SalaryStructures.Queries.ValidateFormula;

public class ValidateFormulaQueryHandler : IRequestHandler<ValidateFormulaQuery, FormulaValidationResult>
{
    private readonly IFormulaEvaluator _formulaEvaluator;

    public ValidateFormulaQueryHandler(IFormulaEvaluator formulaEvaluator)
    {
        _formulaEvaluator = formulaEvaluator;
    }

    public Task<FormulaValidationResult> Handle(ValidateFormulaQuery request, CancellationToken cancellationToken)
    {
        var availableVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BASE", "GROSS", "EARNINGS", "WORKING_DAYS", "WORKED_HOURS", "OVERTIME_HOURS", "LATE_MINUTES", "ABSENT_DAYS"
        };

        if (request.CustomAvailableVariables != null)
        {
            foreach (var variable in request.CustomAvailableVariables)
            {
                availableVariables.Add(variable);
            }
        }

        var validationErrors = _formulaEvaluator.Validate(request.FormulaExpression, availableVariables);
        bool isValid = validationErrors.Count == 0;
        string? errorMessage = isValid ? null : string.Join("; ", validationErrors);

        decimal? sampleResult = null;
        if (isValid)
        {
            try
            {
                var context = new FormulaContext
                {
                    BaseSalary = 50000m,
                    GrossSalary = 75000m,
                    WorkingDays = 22,
                    CalendarWorkingDays = 22,
                    OvertimeHours = 10m,
                    LateMinutes = 15,
                    AbsentDays = 1
                };

                sampleResult = _formulaEvaluator.Evaluate(request.FormulaExpression, context);
            }
            catch
            {
                // Sample evaluation optional if dry-run throws runtime math issue
            }
        }

        return Task.FromResult(new FormulaValidationResult(
            isValid,
            errorMessage,
            availableVariables.ToList(),
            sampleResult));
    }
}

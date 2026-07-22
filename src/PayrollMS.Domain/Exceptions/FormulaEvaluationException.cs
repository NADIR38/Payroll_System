namespace PayrollMS.Domain.Exceptions;

/// <summary>
/// Thrown by <see cref="Interfaces.Services.IFormulaEvaluator"/> when an NCalc expression
/// fails at runtime (e.g. division by zero, type mismatch, disallowed function call).
/// Distinct from <see cref="BusinessRuleViolationException"/> which covers save-time validation.
/// </summary>
public class FormulaEvaluationException : DomainException
{
    public string Expression { get; }
    public string ErrorCode { get; }

    public FormulaEvaluationException(string errorCode, string expression, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        Expression = expression;
    }
}

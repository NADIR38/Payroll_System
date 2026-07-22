using PayrollMS.Domain.Enums;

namespace PayrollMS.Domain.Exceptions;

/// <summary>
/// Thrown when a PayrollRun state transition is invalid per the state machine — PRD §15.3.
/// Maps to HTTP 409 Conflict.
/// </summary>
public class InvalidPayrollStateTransitionException : DomainException
{
    public string ErrorCode => "INVALID_PAYROLL_STATE_TRANSITION";
    public PayrollRunStatus CurrentStatus { get; }
    public PayrollRunStatus AttemptedStatus { get; }

    public InvalidPayrollStateTransitionException(PayrollRunStatus current, PayrollRunStatus attempted)
        : base($"Cannot transition PayrollRun from '{current}' to '{attempted}'. " +
               "See PRD §15.3 for valid transitions.")
    {
        CurrentStatus = current;
        AttemptedStatus = attempted;
    }
}

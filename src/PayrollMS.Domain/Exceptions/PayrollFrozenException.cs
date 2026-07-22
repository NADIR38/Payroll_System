using PayrollMS.Domain.Common;

namespace PayrollMS.Domain.Exceptions;

/// <summary>
/// Thrown when an operation attempts to modify a PayrollRun or PayrollEntry that has
/// moved past the GENERATED status — per AGENTS.md hard rules and PRD §2.4.
/// Maps to HTTP 409 Conflict.
/// </summary>
public class PayrollFrozenException : DomainException
{
    public string ErrorCode => "PAYROLL_ENTRY_LOCKED";

    public PayrollFrozenException(string entityType, string entityId)
        : base($"{entityType} '{entityId}' is frozen and cannot be modified. " +
               "Corrections must use a Correction run (PRD §18.4).")
    {
    }

    public PayrollFrozenException(string message)
        : base(message)
    {
    }
}

namespace PayrollMS.Domain.Enums;

/// <summary>
/// Determines how a salary component rolls up into Gross vs. Net.
/// PRD §8.3
/// </summary>
public enum ComponentType
{
    /// <summary>Adds to gross salary.</summary>
    Earning = 0,

    /// <summary>Subtracts from gross salary to arrive at net.</summary>
    Deduction = 1,

    /// <summary>Employer-side cost (e.g. EOBI); not included in employee net salary.</summary>
    EmployerContribution = 2
}

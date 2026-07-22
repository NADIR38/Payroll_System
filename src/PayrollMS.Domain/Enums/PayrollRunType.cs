namespace PayrollMS.Domain.Enums;

/// <summary>
/// Classifies a PayrollRun by its purpose — PRD §15.2
/// </summary>
public enum PayrollRunType
{
    /// <summary>Standard end-of-month payroll for all (or filtered) employees.</summary>
    Regular = 0,

    /// <summary>An additional run in the same period (e.g. mid-month special payout).</summary>
    Supplementary = 1,

    /// <summary>
    /// A corrective re-run for a specific employee arising from a dispute.
    /// Always has ParentRunId pointing to the original run, and Version = parent.Version + 1.
    /// PRD §18.4
    /// </summary>
    Correction = 2
}

namespace PayrollMS.Domain.Enums;

/// <summary>
/// Per-employee entry status within a PayrollRun — PRD §15.2
/// </summary>
public enum PayrollEntryStatus
{
    /// <summary>Formula engine ran successfully. Entry is ready for review.</summary>
    Calculated = 0,

    /// <summary>
    /// No AttendanceSummary was found for this employee and period.
    /// HR must resolve before the run can be approved.
    /// </summary>
    AttendanceMissing = 1,

    /// <summary>
    /// Employee has no SalaryStructure assigned.
    /// HR must resolve before the run can be approved.
    /// </summary>
    StructureMissing = 2,

    /// <summary>A DisputeTicket has been raised against this entry.</summary>
    Disputed = 3,

    /// <summary>Entry was revised via a Correction run. The original entry is superseded.</summary>
    Revised = 4,

    /// <summary>Entry is locked post-disbursement. No further edits permitted.</summary>
    Locked = 5
}

namespace PayrollMS.Domain.Enums;

/// <summary>
/// The action taken by an approver on a workflow step — PRD §16.2
/// </summary>
public enum ApprovalAction
{
    /// <summary>Step was approved; workflow advances to the next step.</summary>
    Approved = 0,

    /// <summary>Step was rejected; PayrollRun is sent back to Generated status.</summary>
    Rejected = 1,

    /// <summary>Optional step was skipped by the approver.</summary>
    Skipped = 2
}

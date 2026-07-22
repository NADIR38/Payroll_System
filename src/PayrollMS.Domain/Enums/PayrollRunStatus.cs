namespace PayrollMS.Domain.Enums;

/// <summary>
/// Status state machine for a PayrollRun — PRD §15.3
///
/// Valid transitions:
///   Draft → Generating (background job picks it up)
///   Generating → Generated (job completes successfully)
///   Generating → Failed (job throws unhandled exception)
///   Generated → UnderReview (first approval step submitted)
///   Generated | UnderReview → Disputed (at least one entry raises a dispute)
///   Generated | UnderReview → Cancelled (HR cancels before approval)
///   UnderReview → Approved (all workflow steps completed)
///   Approved → DisbursementPending (disbursement batch created)
///   DisbursementPending → Disbursed (bank CSV confirmed)
///   Disbursed → Closed
/// </summary>
public enum PayrollRunStatus
{
    /// <summary>Initial state — run record created, Hangfire job not yet started.</summary>
    Draft = 0,

    /// <summary>Transient — background worker is actively generating entries.</summary>
    Generating = 1,

    /// <summary>Worker completed. Entries are available for review. Snapshots are now frozen.</summary>
    Generated = 2,

    /// <summary>At least one approval step has been submitted; waiting for remaining steps.</summary>
    UnderReview = 3,

    /// <summary>All configured approval workflow steps have been approved.</summary>
    Approved = 4,

    /// <summary>A DisbursementBatch has been created; awaiting bank file download + confirmation.</summary>
    DisbursementPending = 5,

    /// <summary>Bank transfer confirmed by Finance officer.</summary>
    Disbursed = 6,

    /// <summary>Payroll period fully closed. No further actions allowed.</summary>
    Closed = 7,

    /// <summary>Side state — at least one PayrollEntry has an open DisputeTicket.</summary>
    Disputed = 8,

    /// <summary>Run cancelled before disbursement. Cannot be reactivated.</summary>
    Cancelled = 9,

    /// <summary>Background worker failed with an unhandled exception. Details in Remarks.</summary>
    Failed = 10
}

using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Payroll;

/// <summary>
/// Aggregate Root for a batch payroll calculation.
/// One PayrollRun = one month's payroll for a company (or filtered branch/department).
/// PRD §15.1, §15.2, §15.3
///
/// State machine (PRD §15.3):
///   Draft → Generating → Generated → UnderReview → Approved → DisbursementPending → Disbursed → Closed
///   Side states: Disputed, Cancelled, Failed
///
/// Hard rules (AGENTS.md):
///   - Never hard-delete a row from payroll_* tables
///   - Never edit PayrollEntry/Component once Status &gt; GENERATED — requires a Correction run
///   - Corrections create a new versioned run, never overwrite the original
/// </summary>
public sealed class PayrollRun : BaseAuditableEntity<PayrollRunId>, IAggregateRoot
{
    private readonly List<PayrollEntry> _entries = [];

    private PayrollRun() { }

    public CompanyId CompanyId { get; private set; }
    public FinancialYearId FinancialYearId { get; private set; }

    public int PeriodYear { get; private set; }
    public int PeriodMonth { get; private set; }

    public PayrollRunStatus Status { get; private set; }
    public PayrollRunType RunType { get; private set; }

    /// <summary>If set, only employees in this branch are included in the run.</summary>
    public BranchId? FilterBranchId { get; private set; }

    /// <summary>If set, only employees in this department are included in the run.</summary>
    public DepartmentId? FilterDepartmentId { get; private set; }

    // ── Aggregated totals (denormalised — PRD erd-explained §PayrollRun) ──────

    public int TotalEmployees { get; private set; }

    /// <summary>Stored (not computed on read) for dashboard and webhook payloads — PRD erd-explained.</summary>
    public decimal TotalGross { get; private set; }
    public decimal TotalDeductions { get; private set; }
    public decimal TotalNet { get; private set; }

    public DateTimeOffset? GeneratedAt { get; private set; }
    public string? GeneratedBy { get; private set; }
    public string? Remarks { get; private set; }

    /// <summary>
    /// Payroll version number for this period. Starts at 1.
    /// Increments each time a Correction run is created for the same period.
    /// PRD §15.2, erd-explained §PayrollRun
    /// Distinct from BaseEntity.Version (uint EF concurrency token).
    /// </summary>
    public int RunVersion { get; private set; }

    /// <summary>
    /// For Correction runs: points to the original PayrollRun.
    /// PRD §18.4, erd-explained §PayrollRun
    /// </summary>
    public PayrollRunId? ParentRunId { get; private set; }

    public IReadOnlyCollection<PayrollEntry> Entries => _entries.AsReadOnly();

    // ── State machine transition table ────────────────────────────────────────

    private static readonly IReadOnlyDictionary<PayrollRunStatus, IReadOnlySet<PayrollRunStatus>> ValidTransitions =
        new Dictionary<PayrollRunStatus, IReadOnlySet<PayrollRunStatus>>
        {
            [PayrollRunStatus.Draft] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Generating, PayrollRunStatus.Cancelled },
            [PayrollRunStatus.Generating] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Generated, PayrollRunStatus.Failed },
            [PayrollRunStatus.Generated] = new HashSet<PayrollRunStatus> { PayrollRunStatus.UnderReview, PayrollRunStatus.Approved, PayrollRunStatus.Disputed, PayrollRunStatus.Cancelled },
            [PayrollRunStatus.UnderReview] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Generated, PayrollRunStatus.Approved, PayrollRunStatus.Disputed, PayrollRunStatus.Cancelled },
            [PayrollRunStatus.Disputed] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Generated, PayrollRunStatus.UnderReview },
            [PayrollRunStatus.Approved] = new HashSet<PayrollRunStatus> { PayrollRunStatus.DisbursementPending },
            [PayrollRunStatus.DisbursementPending] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Disbursed },
            [PayrollRunStatus.Disbursed] = new HashSet<PayrollRunStatus> { PayrollRunStatus.Closed },
            [PayrollRunStatus.Closed] = new HashSet<PayrollRunStatus>(),
            [PayrollRunStatus.Cancelled] = new HashSet<PayrollRunStatus>(),
            [PayrollRunStatus.Failed] = new HashSet<PayrollRunStatus>()
        };

    // ── Factory: Create a new Regular run ────────────────────────────────────

    /// <summary>
    /// Creates a new Regular (or Supplementary) PayrollRun in DRAFT status.
    /// The caller must have already verified no active Regular run exists for the same period — PRD §15.4
    /// </summary>
    public static PayrollRun Create(
        CompanyId companyId,
        FinancialYearId financialYearId,
        int periodYear,
        int periodMonth,
        PayrollRunType runType,
        string createdBy,
        BranchId? filterBranchId = null,
        DepartmentId? filterDepartmentId = null,
        string? remarks = null)
    {
        ValidateCreateArgs(companyId, financialYearId, periodYear, periodMonth, createdBy);

        var run = new PayrollRun
        {
            Id = PayrollRunId.New(),
            CompanyId = companyId,
            FinancialYearId = financialYearId,
            PeriodYear = periodYear,
            PeriodMonth = periodMonth,
            Status = PayrollRunStatus.Draft,
            RunType = runType,
            FilterBranchId = filterBranchId,
            FilterDepartmentId = filterDepartmentId,
            Remarks = remarks?.Trim(),
            RunVersion = 1,
            ParentRunId = null,
            TotalEmployees = 0,
            TotalGross = 0,
            TotalDeductions = 0,
            TotalNet = 0
        };

        run.SetCreatedBy(createdBy);

        run.AddDomainEvent(new PayrollRunCreatedEvent(
            run.Id, companyId, periodYear, periodMonth, runType));

        return run;
    }

    // ── Factory: Create a Correction run ─────────────────────────────────────

    /// <summary>
    /// Creates a Correction run tied to the original run.
    /// PRD §18.4: Version = parent.Version + 1, ParentRunId = parent.Id
    /// </summary>
    public static PayrollRun CreateCorrection(
        PayrollRun parentRun,
        string createdBy,
        string? remarks = null)
    {
        if (parentRun is null)
            throw new BusinessRuleViolationException("ParentRunRequired", "A correction run must reference a parent run.");

        if (parentRun.Status is PayrollRunStatus.Draft or PayrollRunStatus.Generating or PayrollRunStatus.Failed)
            throw new BusinessRuleViolationException("InvalidParentStatus",
                "Cannot create a correction run for a parent run that has not been generated.");

        var run = new PayrollRun
        {
            Id = PayrollRunId.New(),
            CompanyId = parentRun.CompanyId,
            FinancialYearId = parentRun.FinancialYearId,
            PeriodYear = parentRun.PeriodYear,
            PeriodMonth = parentRun.PeriodMonth,
            Status = PayrollRunStatus.Draft,
            RunType = PayrollRunType.Correction,
            FilterBranchId = null,
            FilterDepartmentId = null,
            Remarks = remarks?.Trim(),
            RunVersion = parentRun.RunVersion + 1,
            ParentRunId = parentRun.Id,
            TotalEmployees = 0,
            TotalGross = 0,
            TotalDeductions = 0,
            TotalNet = 0
        };

        run.SetCreatedBy(createdBy);

        run.AddDomainEvent(new PayrollRunCreatedEvent(
            run.Id, parentRun.CompanyId, parentRun.PeriodYear, parentRun.PeriodMonth, PayrollRunType.Correction));

        return run;
    }

    // ── Status transitions ────────────────────────────────────────────────────

    /// <summary>
    /// Transitions to Generating. Called by the Hangfire worker at the start of execution.
    /// </summary>
    public void MarkGenerating()
    {
        TransitionTo(PayrollRunStatus.Generating);
    }

    /// <summary>
    /// Called by the Hangfire worker when all entries have been calculated.
    /// Snapshots the aggregated totals — PRD §15.5
    /// </summary>
    public void MarkGenerated(
        int totalEmployees,
        decimal totalGross,
        decimal totalDeductions,
        decimal totalNet,
        string generatedBy)
    {
        TransitionTo(PayrollRunStatus.Generated);

        TotalEmployees = totalEmployees;
        TotalGross = totalGross;
        TotalDeductions = totalDeductions;
        TotalNet = totalNet;
        GeneratedAt = DateTimeOffset.UtcNow;
        GeneratedBy = generatedBy;

        SetUpdatedAt();

        AddDomainEvent(new PayrollRunGeneratedEvent(
            Id, CompanyId, PeriodYear, PeriodMonth, totalEmployees, totalGross, totalNet));
    }

    /// <summary>
    /// Called when the background worker encounters an unhandled exception.
    /// Sets Status = Failed and records the error — PRD §15.6
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        TransitionTo(PayrollRunStatus.Failed);
        Remarks = $"[FAILED] {errorMessage}";
        SetUpdatedAt();

        AddDomainEvent(new PayrollRunFailedEvent(Id, CompanyId, errorMessage));
    }

    /// <summary>
    /// Advances the run to UnderReview (first approval step submitted).
    /// </summary>
    public void MarkUnderReview()
    {
        TransitionTo(PayrollRunStatus.UnderReview);
        SetUpdatedAt();
    }

    /// <summary>
    /// Fully approves the run after all workflow steps are complete.
    /// Triggers salary slip generation — PRD §15.3, §17.4
    /// </summary>
    public void MarkApproved(string approvedBy)
    {
        TransitionTo(PayrollRunStatus.Approved);
        SetUpdatedBy(approvedBy);
        SetUpdatedAt();

        AddDomainEvent(new PayrollRunApprovedEvent(
            Id, CompanyId, PeriodYear, PeriodMonth, approvedBy));
    }

    /// <summary>
    /// Moves run back to Generated after a rejection.
    /// Called by the approval handler when any step is rejected — PRD §16.5
    /// </summary>
    public void MarkRejectedToGenerated(string rejectedBy, string stepName, string comments)
    {
        // From UnderReview back to Generated
        if (Status != PayrollRunStatus.UnderReview && Status != PayrollRunStatus.Generated)
            throw new InvalidPayrollStateTransitionException(Status, PayrollRunStatus.Generated);

        Status = PayrollRunStatus.Generated;
        SetUpdatedAt();

        AddDomainEvent(new PayrollRunRejectedEvent(Id, CompanyId, rejectedBy, stepName, comments));
    }

    /// <summary>
    /// Marks the run as Disputed when at least one entry has an open dispute — PRD §15.3
    /// </summary>
    public void MarkDisputed()
    {
        if (Status is PayrollRunStatus.Generated or PayrollRunStatus.UnderReview)
        {
            Status = PayrollRunStatus.Disputed;
            SetUpdatedAt();
        }
    }

    public void MarkDisbursementPending()
    {
        TransitionTo(PayrollRunStatus.DisbursementPending);
        SetUpdatedAt();
    }

    public void MarkDisbursed()
    {
        TransitionTo(PayrollRunStatus.Disbursed);
        SetUpdatedAt();

        // Lock all entries
        foreach (var entry in _entries.Where(e => e.Status == PayrollEntryStatus.Calculated))
            entry.Lock();
    }

    public void MarkClosed()
    {
        TransitionTo(PayrollRunStatus.Closed);
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancels the run before disbursement. Not reversible — PRD §15.3
    /// </summary>
    public void Cancel(string cancelledBy, string reason)
    {
        TransitionTo(PayrollRunStatus.Cancelled);
        Remarks = $"[CANCELLED by {cancelledBy}] {reason}";
        SetUpdatedAt();

        AddDomainEvent(new PayrollRunCancelledEvent(Id, CompanyId, cancelledBy, reason));
    }

    // ── Recalculate totals (called after entry override) ─────────────────────

    /// <summary>
    /// Recalculates run-level totals after a component override on one entry.
    /// </summary>
    public void RecalculateTotals()
    {
        var calculatedEntries = _entries.Where(e =>
            e.Status is PayrollEntryStatus.Calculated
                or PayrollEntryStatus.Disputed
                or PayrollEntryStatus.Revised).ToList();

        TotalGross = calculatedEntries.Sum(e => e.GrossSalary);
        TotalDeductions = calculatedEntries.Sum(e => e.TotalDeductions);
        TotalNet = calculatedEntries.Sum(e => e.NetSalary);
        SetUpdatedAt();
    }

    // ── Guard: is modification of entries allowed? ────────────────────────────

    /// <summary>
    /// Returns true if entries may still be manually overridden.
    /// AGENTS.md hard rule: only GENERATED or UNDER_REVIEW.
    /// </summary>
    public bool CanModifyEntries =>
        Status is PayrollRunStatus.Generated or PayrollRunStatus.UnderReview;

    // ── Private helpers ───────────────────────────────────────────────────────

    private void TransitionTo(PayrollRunStatus newStatus)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidPayrollStateTransitionException(Status, newStatus);

        Status = newStatus;
    }

    private static void ValidateCreateArgs(
        CompanyId companyId,
        FinancialYearId financialYearId,
        int periodYear,
        int periodMonth,
        string createdBy)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "PayrollRun must belong to a company.");

        if (financialYearId == FinancialYearId.Empty)
            throw new BusinessRuleViolationException("FinancialYearRequired", "A FinancialYear must be specified for the payroll run.");

        if (periodMonth is < 1 or > 12)
            throw new BusinessRuleViolationException("InvalidMonth", "PeriodMonth must be between 1 and 12.");

        if (periodYear < 2000 || periodYear > 2100)
            throw new BusinessRuleViolationException("InvalidYear", "PeriodYear is out of range.");

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new BusinessRuleViolationException("CreatedByRequired", "CreatedBy is required.");
    }
}

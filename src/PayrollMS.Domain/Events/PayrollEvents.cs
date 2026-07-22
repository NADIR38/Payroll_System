using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Domain.Events;

/// <summary>
/// Fired when a new PayrollRun is created and queued for generation.
/// PRD §15.4
/// </summary>
public sealed record PayrollRunCreatedEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    int PeriodYear,
    int PeriodMonth,
    PayrollRunType RunType) : IDomainEvent;

/// <summary>
/// Fired when the Hangfire worker successfully completes generation.
/// Triggers HR notification + outbound webhook — PRD §15.5
/// </summary>
public sealed record PayrollRunGeneratedEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    int PeriodYear,
    int PeriodMonth,
    int TotalEmployees,
    decimal TotalGross,
    decimal TotalNet) : IDomainEvent;

/// <summary>
/// Fired when a PayrollRun moves to APPROVED after all workflow steps.
/// Triggers salary slip generation job — PRD §17.4
/// </summary>
public sealed record PayrollRunApprovedEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    int PeriodYear,
    int PeriodMonth,
    string ApprovedBy) : IDomainEvent;

/// <summary>
/// Fired when a PayrollRun is rejected at any approval step.
/// Triggers HR notification — PRD §16.5
/// </summary>
public sealed record PayrollRunRejectedEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    string RejectedBy,
    string StepName,
    string Comments) : IDomainEvent;

/// <summary>
/// Fired when a PayrollRun is cancelled before disbursement.
/// </summary>
public sealed record PayrollRunCancelledEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    string CancelledBy,
    string Reason) : IDomainEvent;

/// <summary>
/// Fired when a PayrollRun generation fails in the background worker.
/// PRD §15.6
/// </summary>
public sealed record PayrollRunFailedEvent(
    PayrollRunId PayrollRunId,
    CompanyId CompanyId,
    string ErrorMessage) : IDomainEvent;

/// <summary>
/// Fired when an HR user manually overrides a PayrollEntryComponent amount.
/// Must be audit-logged — PRD §15.7, §24.3
/// </summary>
public sealed record PayrollEntryComponentOverriddenEvent(
    PayrollEntryComponentId ComponentId,
    PayrollEntryId EntryId,
    PayrollRunId RunId,
    CompanyId CompanyId,
    string ComponentCode,
    decimal OldAmount,
    decimal NewAmount,
    string OverriddenBy,
    string Reason) : IDomainEvent;

using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Approval;

/// <summary>
/// An immutable audit record of a single approval action taken against a PayrollRun step.
/// Append-only — PRD §16.2, §24.3
///
/// Snapshots step_order + step_name at record time so that if the workflow template is later
/// edited, the historical record stays accurate — erd-explained §PayrollApprovalRecord
/// </summary>
public sealed class PayrollApprovalRecord : BaseEntity<PayrollApprovalRecordId>
{
    private PayrollApprovalRecord() { }

    public PayrollRunId PayrollRunId { get; private set; }
    public CompanyId CompanyId { get; private set; }
    public ApprovalWorkflowStepId WorkflowStepId { get; private set; }

    /// <summary>Snapshot of the step order at the time of the action.</summary>
    public int StepOrder { get; private set; }

    /// <summary>Snapshot of the step name at the time of the action.</summary>
    public string StepName { get; private set; } = null!;

    public ApprovalAction Action { get; private set; }

    public string ActorUserId { get; private set; } = null!;
    public string ActorName { get; private set; } = null!;

    /// <summary>The actor's role at the time of the action (snapshot from JWT claim).</summary>
    public string ActorRole { get; private set; } = null!;

    /// <summary>Mandatory for Rejected actions — PRD §16.5</summary>
    public string? Comments { get; private set; }

    public DateTimeOffset ActionAt { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static PayrollApprovalRecord Create(
        PayrollRunId payrollRunId,
        CompanyId companyId,
        ApprovalWorkflowStepId workflowStepId,
        int stepOrder,
        string stepName,
        ApprovalAction action,
        string actorUserId,
        string actorName,
        string actorRole,
        string? comments)
    {
        if (payrollRunId == PayrollRunId.Empty)
            throw new BusinessRuleViolationException("RunRequired", "PayrollRunId is required.");

        if (string.IsNullOrWhiteSpace(actorUserId))
            throw new BusinessRuleViolationException("ActorRequired", "ActorUserId is required.");

        if (string.IsNullOrWhiteSpace(actorName))
            throw new BusinessRuleViolationException("ActorNameRequired", "ActorName is required.");

        if (action == ApprovalAction.Rejected && string.IsNullOrWhiteSpace(comments))
            throw new BusinessRuleViolationException("CommentsRequired",
                "Comments are mandatory when rejecting a payroll run — PRD §16.5.");

        return new PayrollApprovalRecord
        {
            Id = PayrollApprovalRecordId.New(),
            PayrollRunId = payrollRunId,
            CompanyId = companyId,
            WorkflowStepId = workflowStepId,
            StepOrder = stepOrder,
            StepName = stepName.Trim(),
            Action = action,
            ActorUserId = actorUserId.Trim(),
            ActorName = actorName.Trim(),
            ActorRole = actorRole.Trim(),
            Comments = comments?.Trim(),
            ActionAt = DateTimeOffset.UtcNow
        };
    }
}

using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Approval;

/// <summary>
/// A single step in an ApprovalWorkflowTemplate.
/// Steps are executed in StepOrder (ASC) — PRD §16.2
/// </summary>
public sealed class ApprovalWorkflowStep : BaseEntity<ApprovalWorkflowStepId>
{
    private ApprovalWorkflowStep() { }

    public ApprovalWorkflowTemplateId WorkflowTemplateId { get; private set; }

    /// <summary>
    /// Execution order. Lower = approved first.
    /// PRD §16.2: "StepOrder (int) — 1 = first to approve"
    /// </summary>
    public int StepOrder { get; private set; }

    /// <summary>E.g. "HR Review", "Finance Approval", "CEO Sign-off"</summary>
    public string StepName { get; private set; } = null!;

    /// <summary>
    /// The role that can approve this step.
    /// Matched against the actor's JWT role claim at runtime — PRD §16.5
    /// </summary>
    public string RequiredRole { get; private set; } = null!;

    /// <summary>
    /// If true, this step may be skipped by the actor.
    /// The `Skip` action is still recorded in PayrollApprovalRecord — PRD §16.2
    /// </summary>
    public bool IsOptional { get; private set; }

    /// <summary>
    /// When set, a notification is fired if the step is not completed within this many hours.
    /// PRD §16.5: "SLA breach triggers an escalation notification"
    /// </summary>
    public int? SLAHours { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    internal static ApprovalWorkflowStep Create(
        ApprovalWorkflowTemplateId templateId,
        int stepOrder,
        string stepName,
        string requiredRole,
        bool isOptional,
        int? slaHours)
    {
        if (stepOrder < 1)
            throw new BusinessRuleViolationException("InvalidStepOrder", "StepOrder must be 1 or greater.");

        if (slaHours.HasValue && slaHours.Value < 1)
            throw new BusinessRuleViolationException("InvalidSLAHours", "SLAHours must be at least 1.");

        return new ApprovalWorkflowStep
        {
            Id = ApprovalWorkflowStepId.New(),
            WorkflowTemplateId = templateId,
            StepOrder = stepOrder,
            StepName = stepName.Trim(),
            RequiredRole = requiredRole.Trim(),
            IsOptional = isOptional,
            SLAHours = slaHours
        };
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public void Update(string stepName, string requiredRole, bool isOptional, int? slaHours)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new BusinessRuleViolationException("StepNameRequired", "Step name is required.");

        if (string.IsNullOrWhiteSpace(requiredRole))
            throw new BusinessRuleViolationException("RequiredRoleRequired", "RequiredRole is required.");

        if (slaHours.HasValue && slaHours.Value < 1)
            throw new BusinessRuleViolationException("InvalidSLAHours", "SLAHours must be at least 1.");

        StepName = stepName.Trim();
        RequiredRole = requiredRole.Trim();
        IsOptional = isOptional;
        SLAHours = slaHours;
        SetUpdatedAt();
    }
}

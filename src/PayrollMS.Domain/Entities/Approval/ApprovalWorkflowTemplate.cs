using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Approval;

/// <summary>
/// A configurable multi-step approval chain for payroll runs.
/// Different companies can define their own sign-off sequences — PRD §16.1, §16.3
///
/// Aggregate Root: owns the Steps collection.
/// </summary>
public sealed class ApprovalWorkflowTemplate : BaseAuditableEntity<ApprovalWorkflowTemplateId>, IAggregateRoot
{
    private readonly List<ApprovalWorkflowStep> _steps = [];

    private ApprovalWorkflowTemplate() { }

    public CompanyId CompanyId { get; private set; }

    /// <summary>Human-readable name, e.g. "Standard Approval", "CEO Required".</summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// If true, this template is automatically used when creating a PayrollRun
    /// unless a specific template is selected.
    /// Only one template per company can be default — enforced by the command handler.
    /// </summary>
    public bool IsDefault { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ApprovalWorkflowStep> Steps => _steps.AsReadOnly();

    // ── Factory ───────────────────────────────────────────────────────────────

    public static ApprovalWorkflowTemplate Create(
        CompanyId companyId,
        string name,
        bool isDefault,
        string createdBy)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired",
                "ApprovalWorkflowTemplate must belong to a company.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired",
                "Workflow template name cannot be empty.");

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new BusinessRuleViolationException("CreatedByRequired", "CreatedBy is required.");

        var template = new ApprovalWorkflowTemplate
        {
            Id = ApprovalWorkflowTemplateId.New(),
            CompanyId = companyId,
            Name = name.Trim(),
            IsDefault = isDefault,
            IsActive = true
        };

        template.SetCreatedBy(createdBy);
        return template;
    }

    // ── Step management ───────────────────────────────────────────────────────

    /// <summary>
    /// Adds an approval step. Steps must have unique StepOrder values — PRD §16.2
    /// </summary>
    public ApprovalWorkflowStep AddStep(
        int stepOrder,
        string stepName,
        string requiredRole,
        bool isOptional = false,
        int? slaHours = null)
    {
        if (_steps.Any(s => s.StepOrder == stepOrder))
            throw new BusinessRuleViolationException("DuplicateStepOrder",
                $"A step with order {stepOrder} already exists in this workflow.");

        if (string.IsNullOrWhiteSpace(stepName))
            throw new BusinessRuleViolationException("StepNameRequired", "Step name is required.");

        if (string.IsNullOrWhiteSpace(requiredRole))
            throw new BusinessRuleViolationException("RequiredRoleRequired", "RequiredRole is required.");

        var step = ApprovalWorkflowStep.Create(Id, stepOrder, stepName, requiredRole, isOptional, slaHours);
        _steps.Add(step);

        SetUpdatedAt();
        return step;
    }

    /// <summary>Removes a step by its ID (only allowed if no PayrollRun has used this template yet — enforce at command level).</summary>
    public void RemoveStep(ApprovalWorkflowStepId stepId)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId)
            ?? throw new BusinessRuleViolationException("StepNotFound", "Workflow step not found.");

        _steps.Remove(step);
        SetUpdatedAt();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public void UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("NameRequired", "Workflow template name cannot be empty.");

        Name = name.Trim();
        SetUpdatedAt();
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        SetUpdatedAt();
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}

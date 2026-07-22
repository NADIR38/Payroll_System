namespace PayrollMS.Application.Features.Approvals;

public sealed record ApprovalWorkflowStepResponse(
    Guid Id,
    Guid WorkflowTemplateId,
    int StepOrder,
    string StepName,
    string RequiredRole,
    bool IsOptional,
    int? SLAHours);

public sealed record ApprovalWorkflowTemplateResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    bool IsDefault,
    bool IsActive,
    IReadOnlyList<ApprovalWorkflowStepResponse> Steps);

public sealed record PayrollApprovalRecordResponse(
    Guid Id,
    Guid PayrollRunId,
    Guid CompanyId,
    Guid WorkflowStepId,
    int StepOrder,
    string StepName,
    string Action,
    string ActorUserId,
    string ActorName,
    string ActorRole,
    string? Comments,
    DateTimeOffset ActionAt);

public sealed record PendingApprovalItemResponse(
    Guid PayrollRunId,
    Guid CompanyId,
    int PeriodYear,
    int PeriodMonth,
    string RunType,
    int TotalEmployees,
    decimal TotalNet,
    int PendingStepOrder,
    string PendingStepName,
    string RequiredRole,
    DateTimeOffset CreatedAt);

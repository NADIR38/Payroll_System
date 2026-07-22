using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.ApprovePayrollRun;

public sealed record ApprovePayrollRunCommand(
    Guid PayrollRunId,
    Guid CompanyId,
    string ActorUserId,
    string ActorName,
    string ActorRole,
    string? Comments = null) : IRequest<PayrollRunResponse>;

public sealed class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, PayrollRunResponse>
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IApprovalWorkflowTemplateRepository _templateRepo;
    private readonly IPayrollApprovalRecordRepository _approvalRecordRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePayrollRunCommandHandler(
        IPayrollRunRepository payrollRunRepo,
        IApprovalWorkflowTemplateRepository templateRepo,
        IPayrollApprovalRecordRepository approvalRecordRepo,
        IUnitOfWork unitOfWork)
    {
        _payrollRunRepo = payrollRunRepo;
        _templateRepo = templateRepo;
        _approvalRecordRepo = approvalRecordRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollRunResponse> Handle(ApprovePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var runId = new PayrollRunId(request.PayrollRunId);
        var companyId = new CompanyId(request.CompanyId);

        var run = await _payrollRunRepo.GetByIdAsync(runId, cancellationToken);
        if (run == null)
            throw new NotFoundException($"PayrollRun '{request.PayrollRunId}' not found.");

        if (run.Status is not (PayrollRunStatus.Generated or PayrollRunStatus.UnderReview))
            throw new BusinessRuleViolationException("InvalidRunStatusForApproval",
                $"PayrollRun status '{run.Status}' is not valid for approval action.");

        // Load workflow template (default or fallback)
        var template = await _templateRepo.GetDefaultAsync(companyId, cancellationToken);
        if (template == null)
        {
            // Auto-seed or direct approval if no template exists — Q2 answer: support both!
            run.MarkApproved(request.ActorName);
            _payrollRunRepo.Update(run);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponse(run);
        }

        var completedOrders = await _approvalRecordRepo.GetCompletedStepOrdersAsync(runId, cancellationToken);
        var nextStep = template.Steps
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault(s => !completedOrders.Contains(s.StepOrder));

        if (nextStep == null)
        {
            // All steps already completed
            run.MarkApproved(request.ActorName);
            _payrollRunRepo.Update(run);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponse(run);
        }

        // Validate actor role matches required role — PRD §16.5
        if (!string.Equals(nextStep.RequiredRole, request.ActorRole, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.ActorRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.ActorRole, "CompanyAdmin", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleViolationException("ApprovalStepUnauthorized",
                $"Role '{request.ActorRole}' is not authorized to approve step '{nextStep.StepName}'. Required role: '{nextStep.RequiredRole}'.");
        }

        // Record approval
        var record = Domain.Entities.Approval.PayrollApprovalRecord.Create(
            runId, companyId, nextStep.Id, nextStep.StepOrder, nextStep.StepName,
            ApprovalAction.Approved, request.ActorUserId, request.ActorName, request.ActorRole, request.Comments);

        await _approvalRecordRepo.AddAsync(record, cancellationToken);

        // Check if this was the last step
        var remainingSteps = template.Steps
            .Where(s => s.StepOrder > nextStep.StepOrder && !completedOrders.Contains(s.StepOrder))
            .ToList();

        if (remainingSteps.Count == 0)
        {
            run.MarkApproved(request.ActorName);
        }
        else
        {
            run.MarkUnderReview();
        }

        _payrollRunRepo.Update(run);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(run);
    }

    private static PayrollRunResponse MapToResponse(PayrollRun run) =>
        new(
            run.Id.Value,
            run.CompanyId.Value,
            run.FinancialYearId.Value,
            run.PeriodYear,
            run.PeriodMonth,
            run.Status.ToString(),
            run.RunType.ToString(),
            run.FilterBranchId?.Value,
            run.FilterDepartmentId?.Value,
            run.TotalEmployees,
            run.TotalGross,
            run.TotalDeductions,
            run.TotalNet,
            run.GeneratedAt,
            run.GeneratedBy,
            run.Remarks,
            run.RunVersion,
            run.ParentRunId?.Value,
            run.CreatedAt);
}

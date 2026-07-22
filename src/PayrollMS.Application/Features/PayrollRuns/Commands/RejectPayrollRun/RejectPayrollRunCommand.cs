using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.RejectPayrollRun;

public sealed record RejectPayrollRunCommand(
    Guid PayrollRunId,
    Guid CompanyId,
    string ActorUserId,
    string ActorName,
    string ActorRole,
    string Comments) : IRequest<PayrollRunResponse>;

public sealed class RejectPayrollRunCommandHandler : IRequestHandler<RejectPayrollRunCommand, PayrollRunResponse>
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IApprovalWorkflowTemplateRepository _templateRepo;
    private readonly IPayrollApprovalRecordRepository _approvalRecordRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RejectPayrollRunCommandHandler(
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

    public async Task<PayrollRunResponse> Handle(RejectPayrollRunCommand request, CancellationToken cancellationToken)
    {
        var runId = new PayrollRunId(request.PayrollRunId);
        var companyId = new CompanyId(request.CompanyId);

        var run = await _payrollRunRepo.GetByIdAsync(runId, cancellationToken);
        if (run == null)
            throw new NotFoundException($"PayrollRun '{request.PayrollRunId}' not found.");

        if (run.Status is not (PayrollRunStatus.Generated or PayrollRunStatus.UnderReview))
            throw new BusinessRuleViolationException("InvalidRunStatusForRejection",
                $"PayrollRun status '{run.Status}' is not valid for rejection.");

        var completedOrders = await _approvalRecordRepo.GetCompletedStepOrdersAsync(runId, cancellationToken);
        var template = await _templateRepo.GetDefaultAsync(companyId, cancellationToken);

        var currentStepName = "Approval Step";
        ApprovalWorkflowStepId stepId = ApprovalWorkflowStepId.New();
        int stepOrder = 1;

        if (template != null)
        {
            var currentStep = template.Steps
                .OrderBy(s => s.StepOrder)
                .FirstOrDefault(s => !completedOrders.Contains(s.StepOrder));

            if (currentStep != null)
            {
                currentStepName = currentStep.StepName;
                stepId = currentStep.Id;
                stepOrder = currentStep.StepOrder;
            }
        }

        // Record rejection
        var record = Domain.Entities.Approval.PayrollApprovalRecord.Create(
            runId, companyId, stepId, stepOrder, currentStepName,
            ApprovalAction.Rejected, request.ActorUserId, request.ActorName, request.ActorRole, request.Comments);

        await _approvalRecordRepo.AddAsync(record, cancellationToken);

        // Rejection returns run to GENERATED status — PRD §16.5
        run.MarkRejectedToGenerated(request.ActorName, currentStepName, request.Comments);
        _payrollRunRepo.Update(run);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayrollRunResponse(
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
}

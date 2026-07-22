using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Approvals.Queries;

public sealed record GetPendingApprovalsQuery(
    Guid CompanyId,
    string UserRole) : IRequest<IReadOnlyList<PendingApprovalItemResponse>>;

public sealed class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, IReadOnlyList<PendingApprovalItemResponse>>
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IApprovalWorkflowTemplateRepository _templateRepo;
    private readonly IPayrollApprovalRecordRepository _approvalRecordRepo;

    public GetPendingApprovalsQueryHandler(
        IPayrollRunRepository payrollRunRepo,
        IApprovalWorkflowTemplateRepository templateRepo,
        IPayrollApprovalRecordRepository approvalRecordRepo)
    {
        _payrollRunRepo = payrollRunRepo;
        _templateRepo = templateRepo;
        _approvalRecordRepo = approvalRecordRepo;
    }

    public async Task<IReadOnlyList<PendingApprovalItemResponse>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        // Fetch runs in Generated or UnderReview status
        var (runs, _) = await _payrollRunRepo.GetPagedAsync(
            companyId, 1, 100, PayrollRunStatus.Generated, cancellationToken: cancellationToken);

        var (underReviewRuns, _) = await _payrollRunRepo.GetPagedAsync(
            companyId, 1, 100, PayrollRunStatus.UnderReview, cancellationToken: cancellationToken);

        var candidateRuns = runs.Concat(underReviewRuns).ToList();
        var template = await _templateRepo.GetDefaultAsync(companyId, cancellationToken);

        var result = new List<PendingApprovalItemResponse>();

        foreach (var run in candidateRuns)
        {
            var completedOrders = await _approvalRecordRepo.GetCompletedStepOrdersAsync(run.Id, cancellationToken);

            string stepName = "HR / Admin Approval";
            string requiredRole = "HRManager";
            int stepOrder = 1;

            if (template != null && template.Steps.Count > 0)
            {
                var nextStep = template.Steps
                    .OrderBy(s => s.StepOrder)
                    .FirstOrDefault(s => !completedOrders.Contains(s.StepOrder));

                if (nextStep == null) continue; // All steps done

                stepName = nextStep.StepName;
                requiredRole = nextStep.RequiredRole;
                stepOrder = nextStep.StepOrder;
            }

            // Filter by user role (or admins)
            if (string.Equals(requiredRole, request.UserRole, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.UserRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.UserRole, "CompanyAdmin", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(new PendingApprovalItemResponse(
                    run.Id.Value,
                    run.CompanyId.Value,
                    run.PeriodYear,
                    run.PeriodMonth,
                    run.RunType.ToString(),
                    run.TotalEmployees,
                    run.TotalNet,
                    stepOrder,
                    stepName,
                    requiredRole,
                    run.CreatedAt));
            }
        }

        return result;
    }
}

using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Queries;

public sealed record GetPayrollRunByIdQuery(Guid PayrollRunId) : IRequest<PayrollRunResponse>;

public sealed class GetPayrollRunByIdQueryHandler : IRequestHandler<GetPayrollRunByIdQuery, PayrollRunResponse>
{
    private readonly IPayrollRunRepository _payrollRunRepo;

    public GetPayrollRunByIdQueryHandler(IPayrollRunRepository payrollRunRepo)
    {
        _payrollRunRepo = payrollRunRepo;
    }

    public async Task<PayrollRunResponse> Handle(GetPayrollRunByIdQuery request, CancellationToken cancellationToken)
    {
        var runId = new PayrollRunId(request.PayrollRunId);

        var run = await _payrollRunRepo.GetByIdAsync(runId, cancellationToken);
        if (run == null)
            throw new NotFoundException($"PayrollRun '{request.PayrollRunId}' not found.");

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

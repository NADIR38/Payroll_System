using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Queries;

public sealed record GetPayrollRunsQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 20,
    PayrollRunStatus? StatusFilter = null,
    int? PeriodYear = null,
    int? PeriodMonth = null) : IRequest<PagedPayrollRunsResponse>;

public sealed record PagedPayrollRunsResponse(
    IReadOnlyList<PayrollRunResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed class GetPayrollRunsQueryHandler : IRequestHandler<GetPayrollRunsQuery, PagedPayrollRunsResponse>
{
    private readonly IPayrollRunRepository _payrollRunRepo;

    public GetPayrollRunsQueryHandler(IPayrollRunRepository payrollRunRepo)
    {
        _payrollRunRepo = payrollRunRepo;
    }

    public async Task<PagedPayrollRunsResponse> Handle(GetPayrollRunsQuery request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        var (items, totalCount) = await _payrollRunRepo.GetPagedAsync(
            companyId,
            request.Page,
            request.PageSize,
            request.StatusFilter,
            request.PeriodYear,
            request.PeriodMonth,
            cancellationToken);

        var responses = items.Select(run => new PayrollRunResponse(
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
            run.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedPayrollRunsResponse(responses, totalCount, request.Page, request.PageSize, totalPages);
    }
}

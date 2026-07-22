using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Queries;

public sealed record GetPayrollEntriesQuery(
    Guid PayrollRunId,
    int Page = 1,
    int PageSize = 20,
    PayrollEntryStatus? StatusFilter = null,
    string? EmployeeSearch = null) : IRequest<PagedPayrollEntriesResponse>;

public sealed record PagedPayrollEntriesResponse(
    IReadOnlyList<PayrollEntryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed class GetPayrollEntriesQueryHandler : IRequestHandler<GetPayrollEntriesQuery, PagedPayrollEntriesResponse>
{
    private readonly IPayrollEntryRepository _entryRepo;

    public GetPayrollEntriesQueryHandler(IPayrollEntryRepository entryRepo)
    {
        _entryRepo = entryRepo;
    }

    public async Task<PagedPayrollEntriesResponse> Handle(GetPayrollEntriesQuery request, CancellationToken cancellationToken)
    {
        var runId = new PayrollRunId(request.PayrollRunId);

        var (items, totalCount) = await _entryRepo.GetPagedByRunIdAsync(
            runId,
            request.Page,
            request.PageSize,
            request.StatusFilter,
            request.EmployeeSearch,
            cancellationToken);

        var responses = items.Select(entry => new PayrollEntryResponse(
            entry.Id.Value,
            entry.PayrollRunId.Value,
            entry.CompanyId.Value,
            entry.ExternalEmployeeId,
            entry.EmployeeCode,
            entry.EmployeeName,
            entry.DepartmentName,
            entry.DesignationName,
            entry.BankName,
            entry.IBAN,
            entry.BaseSalary,
            entry.WorkingDays,
            entry.AbsentDays,
            entry.LateDays,
            entry.GrossSalary,
            entry.TotalDeductions,
            entry.NetSalary,
            entry.Status.ToString(),
            entry.Components.Select(c => new PayrollEntryComponentResponse(
                c.Id.Value,
                c.PayrollEntryId.Value,
                c.SalaryComponentId.Value,
                c.ComponentCode,
                c.ComponentName,
                c.ComponentType.ToString(),
                c.FormulaUsed,
                c.CalculatedAmount,
                c.IsManualOverride)).ToList())).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedPayrollEntriesResponse(responses, totalCount, request.Page, request.PageSize, totalPages);
    }
}

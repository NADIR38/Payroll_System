using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;

namespace PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileHistory;

public class GetEmployeeProfileHistoryQueryHandler : IRequestHandler<GetEmployeeProfileHistoryQuery, IReadOnlyList<EmployeeProfileHistoryResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetEmployeeProfileHistoryQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeProfileHistoryResponse>> Handle(GetEmployeeProfileHistoryQuery request, CancellationToken cancellationToken)
    {
        var targetProfileId = new Domain.Common.EmployeePayrollProfileId(request.EmployeeProfileId);

        var history = await _dbContext.EmployeeProfileHistory
            .AsNoTracking()
            .Where(h => h.EmployeePayrollProfileId == targetProfileId)
            .OrderByDescending(h => h.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return history.Select(h => new EmployeeProfileHistoryResponse(
            h.Id.Value,
            h.EmployeePayrollProfileId.Value,
            h.ExternalEmployeeId,
            h.EmployeeCode,
            h.FullName,
            h.SalaryStructureId.Value,
            h.BaseSalary,
            h.EffectiveFrom,
            h.EffectiveTo,
            h.ChangedBy,
            h.ChangeReason
        )).ToList();
    }
}

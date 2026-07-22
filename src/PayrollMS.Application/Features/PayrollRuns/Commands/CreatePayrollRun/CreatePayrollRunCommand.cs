using MediatR;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.CreatePayrollRun;

public sealed record CreatePayrollRunCommand(
    Guid CompanyId,
    Guid FinancialYearId,
    int PeriodYear,
    int PeriodMonth,
    PayrollRunType RunType = PayrollRunType.Regular,
    Guid? FilterBranchId = null,
    Guid? FilterDepartmentId = null,
    string? Remarks = null,
    string CreatedBy = "System") : IRequest<PayrollRunCreatedResult>;

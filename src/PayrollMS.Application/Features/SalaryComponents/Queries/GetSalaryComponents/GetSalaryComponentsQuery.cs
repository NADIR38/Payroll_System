using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponents;

public sealed record GetSalaryComponentsQuery(
    Guid CompanyId,
    string? TypeFilter = null,      // Optional: "Earning" | "Deduction" | "EmployerContribution"
    bool? IsActiveOnly = true) : IRequest<IReadOnlyList<SalaryComponentResponse>>;

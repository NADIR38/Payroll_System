using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.DeactivateSalaryComponent;

public sealed record DeactivateSalaryComponentCommand(
    Guid ComponentId,
    Guid CompanyId) : IRequest;

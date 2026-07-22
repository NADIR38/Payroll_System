using MediatR;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.SeedPredefinedSalaryComponents;

public sealed record SeedPredefinedSalaryComponentsCommand(
    Guid CompanyId) : IRequest<PredefinedComponentSeedResult>;

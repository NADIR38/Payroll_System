using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetAllowanceRule;

public sealed record SetAllowanceRuleCommand(
    Guid SalaryStructureId,
    Guid CompanyId,
    Guid StructureComponentId,
    string ApplicationMode,    // "Automatic" | "ConditionBased" | "ManualClaim"
    string? ConditionExpression = null,
    string? Description = null) : IRequest<Guid>;

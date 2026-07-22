using MediatR;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.SetDeductionRule;

public sealed record SetDeductionRuleCommand(
    Guid SalaryStructureId,
    Guid CompanyId,
    Guid StructureComponentId,
    string DeductionType,      // "Standard" | "LateArrival" | "Absence" | "LoanRepayment" | "AdvanceRecovery" | "CustomPenalty"
    bool IsOptIn = false,
    int GracePeriodMinutes = 0,
    string? DeductionFormula = null) : IRequest<Guid>;

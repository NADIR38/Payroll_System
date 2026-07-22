using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.PayrollRuns.Commands.OverrideComponent;

public sealed record OverrideComponentCommand(
    Guid PayrollRunId,
    Guid PayrollEntryId,
    Guid ComponentId,
    decimal OverrideAmount,
    string OverriddenBy,
    string Reason) : IRequest<PayrollEntryComponentResponse>;

public sealed class OverrideComponentCommandHandler : IRequestHandler<OverrideComponentCommand, PayrollEntryComponentResponse>
{
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollEntryRepository _entryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OverrideComponentCommandHandler(
        IPayrollRunRepository payrollRunRepo,
        IPayrollEntryRepository entryRepo,
        IUnitOfWork unitOfWork)
    {
        _payrollRunRepo = payrollRunRepo;
        _entryRepo = entryRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollEntryComponentResponse> Handle(OverrideComponentCommand request, CancellationToken cancellationToken)
    {
        var runId = new PayrollRunId(request.PayrollRunId);
        var entryId = new PayrollEntryId(request.PayrollEntryId);
        var compId = new PayrollEntryComponentId(request.ComponentId);

        var run = await _payrollRunRepo.GetByIdAsync(runId, cancellationToken);
        if (run == null)
            throw new NotFoundException($"PayrollRun '{request.PayrollRunId}' not found.");

        var entry = await _entryRepo.GetByIdWithComponentsAsync(entryId, cancellationToken);
        if (entry == null)
            throw new NotFoundException($"PayrollEntry '{request.PayrollEntryId}' not found.");

        // Override component (domain enforces status guard <= GENERATED/UNDER_REVIEW) — PRD §15.7
        var updatedComponent = entry.OverrideComponent(
            compId, request.OverrideAmount, request.OverriddenBy, request.Reason, run.Status);

        _entryRepo.Update(entry);

        // Recalculate run-level totals
        var allEntries = await _entryRepo.GetByRunIdAsync(runId, cancellationToken);
        run.RecalculateTotals();
        _payrollRunRepo.Update(run);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PayrollEntryComponentResponse(
            updatedComponent.Id.Value,
            updatedComponent.PayrollEntryId.Value,
            updatedComponent.SalaryComponentId.Value,
            updatedComponent.ComponentCode,
            updatedComponent.ComponentName,
            updatedComponent.ComponentType.ToString(),
            updatedComponent.FormulaUsed,
            updatedComponent.CalculatedAmount,
            updatedComponent.IsManualOverride);
    }
}

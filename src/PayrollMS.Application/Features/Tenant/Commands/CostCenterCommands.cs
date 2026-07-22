using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE COST CENTER
// ==========================================

public sealed record CreateCostCenterCommand(
    Guid CompanyId,
    string Name,
    string Code) : IRequest<Guid>;

public class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Z0-9_-]{2,20}$")
            .WithMessage("Cost Center code must be 2-20 alphanumeric characters, hyphens, or underscores.");
    }
}

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Guid>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(
        ICostCenterRepository costCenterRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        var existing = await _costCenterRepository.GetByCodeAsync(companyId, request.Code, cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleViolationException("DUPLICATE_COSTCENTER_CODE", $"Cost Center with code '{request.Code}' already exists in this company.");
        }

        var costCenter = CostCenter.Create(
            companyId,
            request.Name,
            request.Code);

        await _costCenterRepository.AddAsync(costCenter, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return costCenter.Id.Value;
    }
}

// ==========================================
// 2. UPDATE COST CENTER DETAILS
// ==========================================

public sealed record UpdateCostCenterDetailsCommand(
    Guid Id,
    string Name) : IRequest;

public class UpdateCostCenterDetailsCommandValidator : AbstractValidator<UpdateCostCenterDetailsCommand>
{
    public UpdateCostCenterDetailsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateCostCenterDetailsCommandHandler : IRequestHandler<UpdateCostCenterDetailsCommand>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCostCenterDetailsCommandHandler(ICostCenterRepository costCenterRepository, IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCostCenterDetailsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var costCenterId = new CostCenterId(request.Id);
        var costCenter = await _costCenterRepository.GetByIdForUpdateAsync(costCenterId, cancellationToken);
        if (costCenter == null)
        {
            throw new NotFoundException($"Cost Center with ID '{request.Id}' was not found.");
        }

        costCenter.UpdateDetails(request.Name);

        _costCenterRepository.Update(costCenter);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 3. ACTIVATE COST CENTER
// ==========================================

public sealed record ActivateCostCenterCommand(Guid Id) : IRequest;

public class ActivateCostCenterCommandHandler : IRequestHandler<ActivateCostCenterCommand>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCostCenterCommandHandler(ICostCenterRepository costCenterRepository, IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var costCenterId = new CostCenterId(request.Id);
        var costCenter = await _costCenterRepository.GetByIdForUpdateAsync(costCenterId, cancellationToken);
        if (costCenter == null)
        {
            throw new NotFoundException($"Cost Center with ID '{request.Id}' was not found.");
        }

        costCenter.Activate();

        _costCenterRepository.Update(costCenter);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 4. DEACTIVATE COST CENTER
// ==========================================

public sealed record DeactivateCostCenterCommand(Guid Id) : IRequest;

public class DeactivateCostCenterCommandHandler : IRequestHandler<DeactivateCostCenterCommand>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCostCenterCommandHandler(ICostCenterRepository costCenterRepository, IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var costCenterId = new CostCenterId(request.Id);
        var costCenter = await _costCenterRepository.GetByIdForUpdateAsync(costCenterId, cancellationToken);
        if (costCenter == null)
        {
            throw new NotFoundException($"Cost Center with ID '{request.Id}' was not found.");
        }

        costCenter.Deactivate();

        _costCenterRepository.Update(costCenter);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

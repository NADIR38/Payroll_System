using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE BRANCH
// ==========================================

public sealed record CreateBranchCommand(
    Guid CompanyId,
    string Name,
    string Code,
    string? Address) : IRequest<Guid>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Z0-9_-]{2,20}$")
            .WithMessage("Branch code must be 2-20 alphanumeric characters, hyphens, or underscores.");
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(
        IBranchRepository branchRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        var existing = await _branchRepository.GetByCodeAsync(companyId, request.Code, cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleViolationException("DUPLICATE_BRANCH_CODE", $"Branch with code '{request.Code}' already exists in this company.");
        }

        var branch = Branch.Create(
            companyId,
            request.Name,
            request.Code,
            request.Address);

        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return branch.Id.Value;
    }
}

// ==========================================
// 2. UPDATE BRANCH DETAILS
// ==========================================

public sealed record UpdateBranchDetailsCommand(
    Guid Id,
    string Name,
    string? Address) : IRequest;

public class UpdateBranchDetailsCommandValidator : AbstractValidator<UpdateBranchDetailsCommand>
{
    public UpdateBranchDetailsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateBranchDetailsCommandHandler : IRequestHandler<UpdateBranchDetailsCommand>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchDetailsCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateBranchDetailsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var branchId = new BranchId(request.Id);
        var branch = await _branchRepository.GetByIdForUpdateAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new NotFoundException($"Branch with ID '{request.Id}' was not found.");
        }

        branch.UpdateDetails(request.Name, request.Address);

        _branchRepository.Update(branch);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 3. ACTIVATE BRANCH
// ==========================================

public sealed record ActivateBranchCommand(Guid Id) : IRequest;

public class ActivateBranchCommandHandler : IRequestHandler<ActivateBranchCommand>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateBranchCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateBranchCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var branchId = new BranchId(request.Id);
        var branch = await _branchRepository.GetByIdForUpdateAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new NotFoundException($"Branch with ID '{request.Id}' was not found.");
        }

        branch.Activate();

        _branchRepository.Update(branch);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 4. DEACTIVATE BRANCH
// ==========================================

public sealed record DeactivateBranchCommand(Guid Id) : IRequest;

public class DeactivateBranchCommandHandler : IRequestHandler<DeactivateBranchCommand>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchCommandHandler(IBranchRepository branchRepository, IUnitOfWork unitOfWork)
    {
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var branchId = new BranchId(request.Id);
        var branch = await _branchRepository.GetByIdForUpdateAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new NotFoundException($"Branch with ID '{request.Id}' was not found.");
        }

        branch.Deactivate();

        _branchRepository.Update(branch);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE DESIGNATION
// ==========================================

public sealed record CreateDesignationCommand(
    Guid CompanyId,
    string Name,
    string Code,
    string? Grade) : IRequest<Guid>;

public class CreateDesignationCommandValidator : AbstractValidator<CreateDesignationCommand>
{
    public CreateDesignationCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Z0-9_-]{2,20}$")
            .WithMessage("Designation code must be 2-20 alphanumeric characters, hyphens, or underscores.");
    }
}

public class CreateDesignationCommandHandler : IRequestHandler<CreateDesignationCommand, Guid>
{
    private readonly IDesignationRepository _designationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDesignationCommandHandler(
        IDesignationRepository designationRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _designationRepository = designationRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        var existing = await _designationRepository.GetByCodeAsync(companyId, request.Code, cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleViolationException("DUPLICATE_DESIGNATION_CODE", $"Designation with code '{request.Code}' already exists in this company.");
        }

        var designation = Designation.Create(
            companyId,
            request.Name,
            request.Code,
            request.Grade);

        await _designationRepository.AddAsync(designation, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return designation.Id.Value;
    }
}

// ==========================================
// 2. UPDATE DESIGNATION DETAILS
// ==========================================

public sealed record UpdateDesignationDetailsCommand(
    Guid Id,
    string Name,
    string? Grade) : IRequest;

public class UpdateDesignationDetailsCommandValidator : AbstractValidator<UpdateDesignationDetailsCommand>
{
    public UpdateDesignationDetailsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateDesignationDetailsCommandHandler : IRequestHandler<UpdateDesignationDetailsCommand>
{
    private readonly IDesignationRepository _designationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDesignationDetailsCommandHandler(IDesignationRepository designationRepository, IUnitOfWork unitOfWork)
    {
        _designationRepository = designationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDesignationDetailsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var designationId = new DesignationId(request.Id);
        var designation = await _designationRepository.GetByIdForUpdateAsync(designationId, cancellationToken);
        if (designation == null)
        {
            throw new NotFoundException($"Designation with ID '{request.Id}' was not found.");
        }

        designation.UpdateDetails(request.Name, request.Grade);

        _designationRepository.Update(designation);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 3. ACTIVATE DESIGNATION
// ==========================================

public sealed record ActivateDesignationCommand(Guid Id) : IRequest;

public class ActivateDesignationCommandHandler : IRequestHandler<ActivateDesignationCommand>
{
    private readonly IDesignationRepository _designationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateDesignationCommandHandler(IDesignationRepository designationRepository, IUnitOfWork unitOfWork)
    {
        _designationRepository = designationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateDesignationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var designationId = new DesignationId(request.Id);
        var designation = await _designationRepository.GetByIdForUpdateAsync(designationId, cancellationToken);
        if (designation == null)
        {
            throw new NotFoundException($"Designation with ID '{request.Id}' was not found.");
        }

        designation.Activate();

        _designationRepository.Update(designation);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 4. DEACTIVATE DESIGNATION
// ==========================================

public sealed record DeactivateDesignationCommand(Guid Id) : IRequest;

public class DeactivateDesignationCommandHandler : IRequestHandler<DeactivateDesignationCommand>
{
    private readonly IDesignationRepository _designationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDesignationCommandHandler(IDesignationRepository designationRepository, IUnitOfWork unitOfWork)
    {
        _designationRepository = designationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateDesignationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var designationId = new DesignationId(request.Id);
        var designation = await _designationRepository.GetByIdForUpdateAsync(designationId, cancellationToken);
        if (designation == null)
        {
            throw new NotFoundException($"Designation with ID '{request.Id}' was not found.");
        }

        designation.Deactivate();

        _designationRepository.Update(designation);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

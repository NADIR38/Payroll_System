using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE DEPARTMENT
// ==========================================

public sealed record CreateDepartmentCommand(
    Guid CompanyId,
    Guid? BranchId,
    string Name,
    string Code) : IRequest<Guid>;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().Matches("^[A-Z0-9_-]{2,20}$")
            .WithMessage("Department code must be 2-20 alphanumeric characters, hyphens, or underscores.");
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        ICompanyRepository companyRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _companyRepository = companyRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        BranchId? branchId = null;
        if (request.BranchId.HasValue)
        {
            branchId = new BranchId(request.BranchId.Value);
            var branch = await _branchRepository.GetByIdForUpdateAsync(branchId.Value, cancellationToken);
            if (branch == null)
            {
                throw new NotFoundException($"Branch with ID '{request.BranchId}' was not found.");
            }
        }

        var existing = await _departmentRepository.GetByCodeAsync(companyId, request.Code, cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleViolationException("DUPLICATE_DEPARTMENT_CODE", $"Department with code '{request.Code}' already exists in this company.");
        }

        // Correct signature: CompanyId companyId, string name, string code, BranchId? branchId = null
        var department = Department.Create(
            companyId,
            request.Name,
            request.Code,
            branchId);

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return department.Id.Value;
    }
}

// ==========================================
// 2. UPDATE DEPARTMENT DETAILS
// ==========================================

public sealed record UpdateDepartmentDetailsCommand(
    Guid Id,
    Guid? BranchId,
    string Name) : IRequest;

public class UpdateDepartmentDetailsCommandValidator : AbstractValidator<UpdateDepartmentDetailsCommand>
{
    public UpdateDepartmentDetailsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public class UpdateDepartmentDetailsCommandHandler : IRequestHandler<UpdateDepartmentDetailsCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentDetailsCommandHandler(
        IDepartmentRepository departmentRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDepartmentDetailsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var departmentId = new DepartmentId(request.Id);
        var department = await _departmentRepository.GetByIdForUpdateAsync(departmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException($"Department with ID '{request.Id}' was not found.");
        }

        BranchId? branchId = null;
        if (request.BranchId.HasValue)
        {
            branchId = new BranchId(request.BranchId.Value);
            var branch = await _branchRepository.GetByIdForUpdateAsync(branchId.Value, cancellationToken);
            if (branch == null)
            {
                throw new NotFoundException($"Branch with ID '{request.BranchId}' was not found.");
            }
        }

        department.UpdateDetails(request.Name, branchId);

        _departmentRepository.Update(department);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 3. ACTIVATE DEPARTMENT
// ==========================================

public sealed record ActivateDepartmentCommand(Guid Id) : IRequest;

public class ActivateDepartmentCommandHandler : IRequestHandler<ActivateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var departmentId = new DepartmentId(request.Id);
        var department = await _departmentRepository.GetByIdForUpdateAsync(departmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException($"Department with ID '{request.Id}' was not found.");
        }

        department.Activate();

        _departmentRepository.Update(department);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

// ==========================================
// 4. DEACTIVATE DEPARTMENT
// ==========================================

public sealed record DeactivateDepartmentCommand(Guid Id) : IRequest;

public class DeactivateDepartmentCommandHandler : IRequestHandler<DeactivateDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var departmentId = new DepartmentId(request.Id);
        var department = await _departmentRepository.GetByIdForUpdateAsync(departmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException($"Department with ID '{request.Id}' was not found.");
        }

        department.Deactivate();

        _departmentRepository.Update(department);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

using FluentValidation;
using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Commands;

// ==========================================
// 1. CREATE FINANCIAL YEAR
// ==========================================

public sealed record CreateFinancialYearCommand(
    Guid CompanyId,
    string Label,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent) : IRequest<Guid>;

public class CreateFinancialYearCommandValidator : AbstractValidator<CreateFinancialYearCommand>
{
    public CreateFinancialYearCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}

public class CreateFinancialYearCommandHandler : IRequestHandler<CreateFinancialYearCommand, Guid>
{
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFinancialYearCommandHandler(
        IFinancialYearRepository financialYearRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _financialYearRepository = financialYearRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFinancialYearCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var company = await _companyRepository.GetByIdForUpdateAsync(companyId, cancellationToken);
        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");
        }

        var existing = await _financialYearRepository.FindAsync(
            f => f.CompanyId == companyId && f.Label == request.Label, 
            cancellationToken);
        if (existing.Count > 0)
        {
            throw new BusinessRuleViolationException("DUPLICATE_FINANCIAL_YEAR", $"Financial Year '{request.Label}' already exists for this company.");
        }

        var financialYear = FinancialYear.Create(
            companyId,
            request.Label,
            request.StartDate,
            request.EndDate);

        if (request.IsCurrent)
        {
            // Deactivate any currently active years
            var activeYears = await _financialYearRepository.FindAsync(
                f => f.CompanyId == companyId && f.IsCurrent, 
                cancellationToken);
            foreach (var activeYear in activeYears)
            {
                await _financialYearRepository.GetByIdForUpdateAsync(activeYear.Id, cancellationToken); // Lock it
                activeYear.RemoveCurrentStatus();
                _financialYearRepository.Update(activeYear);
            }
            financialYear.MarkAsCurrent();
        }

        await _financialYearRepository.AddAsync(financialYear, cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return financialYear.Id.Value;
    }
}

// ==========================================
// 2. MARK FINANCIAL YEAR CURRENT
// ==========================================

public sealed record MarkFinancialYearCurrentCommand(Guid Id) : IRequest;

public class MarkFinancialYearCurrentCommandHandler : IRequestHandler<MarkFinancialYearCurrentCommand>
{
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkFinancialYearCurrentCommandHandler(IFinancialYearRepository financialYearRepository, IUnitOfWork unitOfWork)
    {
        _financialYearRepository = financialYearRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkFinancialYearCurrentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var financialYearId = new FinancialYearId(request.Id);
        var targetYear = await _financialYearRepository.GetByIdForUpdateAsync(financialYearId, cancellationToken);
        if (targetYear == null)
        {
            throw new NotFoundException($"Financial Year with ID '{request.Id}' was not found.");
        }

        // Lock other financial years for this company to prevent concurrent updates
        var otherActiveYears = await _financialYearRepository.FindAsync(
            f => f.CompanyId == targetYear.CompanyId && f.IsCurrent && f.Id != targetYear.Id, 
            cancellationToken);

        foreach (var year in otherActiveYears)
        {
            await _financialYearRepository.GetByIdForUpdateAsync(year.Id, cancellationToken); // Lock row
            year.RemoveCurrentStatus();
            _financialYearRepository.Update(year);
        }

        targetYear.MarkAsCurrent();

        _financialYearRepository.Update(targetYear);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

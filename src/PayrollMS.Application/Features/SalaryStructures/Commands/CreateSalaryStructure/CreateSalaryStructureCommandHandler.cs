using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.CreateSalaryStructure;

public class CreateSalaryStructureCommandHandler : IRequestHandler<CreateSalaryStructureCommand, Guid>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalaryStructureCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        // 1-Query Validation: Verify Company exists AND check duplicate structure Code in a single DB roundtrip
        var validationData = await _dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new
            {
                CompanyExists = true,
                CodeExists = _dbContext.SalaryStructures.Any(s => s.CompanyId == companyId && s.Code == normalizedCode)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (validationData == null)
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");

        if (validationData.CodeExists)
        {
            throw new BusinessRuleViolationException(
                "DUPLICATE_STRUCTURE_CODE",
                $"Salary structure with code '{normalizedCode}' already exists for this company.");
        }

        var structure = SalaryStructure.Create(
            companyId,
            request.Name,
            normalizedCode,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.Description);

        await _salaryStructureRepository.AddAsync(structure, cancellationToken);
        try
        {
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw new BusinessRuleViolationException(
                "DUPLICATE_STRUCTURE_CODE",
                $"Salary structure with code '{normalizedCode}' already exists for this company.");
        }

        return structure.Id.Value;
    }
}

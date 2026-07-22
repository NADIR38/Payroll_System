using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.CreateSalaryComponent;

public class CreateSalaryComponentCommandHandler : IRequestHandler<CreateSalaryComponentCommand, Guid>
{
    private readonly ISalaryComponentRepository _salaryComponentRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalaryComponentCommandHandler(
        ISalaryComponentRepository salaryComponentRepository,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _salaryComponentRepository = salaryComponentRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSalaryComponentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        // 1-Query Validation: Verify Company exists AND check duplicate Code concurrently to prevent N+1 DB calls
        var validationData = await _dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new
            {
                CompanyExists = true,
                CodeExists = _dbContext.SalaryComponents.Any(s => s.CompanyId == companyId && s.Code == normalizedCode)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (validationData == null)
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");

        if (validationData.CodeExists)
        {
            throw new BusinessRuleViolationException(
                "DUPLICATE_COMPONENT_CODE",
                $"Salary component with code '{normalizedCode}' already exists for this company.");
        }

        var type = Enum.Parse<ComponentType>(request.Type, ignoreCase: true);
        var method = Enum.Parse<CalculationMethod>(request.CalculationMethod, ignoreCase: true);

        var component = SalaryComponent.Create(
            companyId,
            request.Name,
            normalizedCode,
            type,
            method,
            request.DefaultValue,
            request.IsTaxable,
            request.IsRecurring,
            request.IsOptional,
            request.Description);

        await _salaryComponentRepository.AddAsync(component, cancellationToken);
        try
        {
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw new BusinessRuleViolationException(
                "DUPLICATE_COMPONENT_CODE",
                $"Salary component with code '{normalizedCode}' already exists for this company.");
        }

        return component.Id.Value;
    }
}

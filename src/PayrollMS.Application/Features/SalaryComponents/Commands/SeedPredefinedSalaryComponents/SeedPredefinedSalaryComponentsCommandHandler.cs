using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.SeedPredefinedSalaryComponents;

public class SeedPredefinedSalaryComponentsCommandHandler : IRequestHandler<SeedPredefinedSalaryComponentsCommand, PredefinedComponentSeedResult>
{
    private readonly ISalaryComponentRepository _salaryComponentRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public SeedPredefinedSalaryComponentsCommandHandler(
        ISalaryComponentRepository salaryComponentRepository,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _salaryComponentRepository = salaryComponentRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<PredefinedComponentSeedResult> Handle(SeedPredefinedSalaryComponentsCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);

        // 1. Verify Company exists
        var companyExists = await _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(c => c.Id == companyId, cancellationToken);

        if (!companyExists)
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");

        // 2. OPTIMIZATION: 1-Query fetch of existing component codes for this company to prevent 14 N+1 DB calls
        var existingCodes = (await _dbContext.SalaryComponents
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .Select(s => s.Code)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 3. Define the 14 standard predefined components from PRD §8.5
        var predefinedComponents = GetPredefinedComponentsList(companyId);

        var seededCodes = new List<string>();

        foreach (var def in predefinedComponents)
        {
            if (!existingCodes.Contains(def.Code))
            {
                var component = SalaryComponent.Create(
                    companyId,
                    def.Name,
                    def.Code,
                    def.Type,
                    def.CalculationMethod,
                    def.DefaultValue,
                    def.IsTaxable,
                    def.IsRecurring,
                    def.IsOptional,
                    def.Description);

                await _salaryComponentRepository.AddAsync(component, cancellationToken);
                seededCodes.Add(def.Code);
            }
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new PredefinedComponentSeedResult(
            TotalPredefinedCount: predefinedComponents.Count,
            NewlySeededCount: seededCodes.Count,
            SeededCodes: seededCodes);
    }

    private static List<PredefinedDefinition> GetPredefinedComponentsList(CompanyId companyId) => new()
    {
        new("Basic Salary", "BASIC", ComponentType.Earning, CalculationMethod.Fixed, null, true, true, false, "Base salary of employee"),
        new("House Rent Allowance", "HOUSE_RENT", ComponentType.Earning, CalculationMethod.PercentageOfBase, null, true, true, false, "House rent allowance"),
        new("Medical Allowance", "MEDICAL", ComponentType.Earning, CalculationMethod.Fixed, null, true, true, false, "Medical allowance"),
        new("Transport Allowance", "TRANSPORT", ComponentType.Earning, CalculationMethod.Fixed, null, true, true, true, "Conveyance & transport allowance"),
        new("Travel Allowance", "TRAVEL", ComponentType.Earning, CalculationMethod.Fixed, null, false, false, true, "Business travel allowance"),
        new("Fuel Allowance", "FUEL", ComponentType.Earning, CalculationMethod.Fixed, null, true, true, true, "Fuel reimbursement allowance"),
        new("Internet Allowance", "INTERNET", ComponentType.Earning, CalculationMethod.Fixed, null, false, true, true, "Home internet utility allowance"),
        new("Utility Allowance", "UTILITY", ComponentType.Earning, CalculationMethod.Fixed, null, true, true, true, "General utilities allowance"),
        new("Overtime Pay", "OVERTIME", ComponentType.Earning, CalculationMethod.PerHour, null, true, false, true, "Overtime pay per worked hour"),
        new("Performance Bonus", "BONUS", ComponentType.Earning, CalculationMethod.Fixed, null, true, false, true, "Discretionary performance bonus"),
        new("Late Arrival Deduction", "LATE_DEDUCT", ComponentType.Deduction, CalculationMethod.FormulaExpression, null, false, false, true, "Late clock-in penalty deduction"),
        new("Absence Deduction", "ABSENT_DEDUCT", ComponentType.Deduction, CalculationMethod.PerWorkingDay, null, false, false, true, "Unapproved absence deduction per day"),
        new("Loan Repayment Deduction", "LOAN_DEDUCT", ComponentType.Deduction, CalculationMethod.Fixed, null, false, true, true, "Monthly company loan installment"),
        new("Salary Advance Recovery", "ADVANCE_DEDUCT", ComponentType.Deduction, CalculationMethod.Fixed, null, false, false, true, "Salary advance recovery deduction")
    };

    private record PredefinedDefinition(
        string Name,
        string Code,
        ComponentType Type,
        CalculationMethod CalculationMethod,
        decimal? DefaultValue,
        bool IsTaxable,
        bool IsRecurring,
        bool IsOptional,
        string Description);
}

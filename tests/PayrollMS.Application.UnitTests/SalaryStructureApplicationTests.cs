using FluentAssertions;
using NSubstitute;
using PayrollMS.Application.Features.SalaryStructures.Commands.AddComponentToStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.CreateSalaryStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.DeactivateSalaryStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.RemoveComponentFromStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.SetAllowanceRule;
using PayrollMS.Application.Features.SalaryStructures.Commands.SetDeductionRule;
using PayrollMS.Application.Features.SalaryStructures.Queries.ValidateFormula;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;
using Xunit;

namespace PayrollMS.Application.UnitTests;

public class SalaryStructureApplicationTests
{
    private readonly ISalaryStructureRepository _salaryStructureRepository = Substitute.For<ISalaryStructureRepository>();
    private readonly IFormulaEvaluator _formulaEvaluator = Substitute.For<IFormulaEvaluator>();
    private readonly IAppDbContext _dbContext = Substitute.For<IAppDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateSalaryStructure_ShouldReturnNewId_WhenValid()
    {
        // Arrange
        var company = Company.Create("Acme", "ACME");
        var companyId = company.Id;
        _dbContext.Companies.Returns(new List<Company> { company }.AsQueryable());
        _dbContext.SalaryStructures.Returns(new List<SalaryStructure>().AsQueryable());

        var command = new CreateSalaryStructureCommand(
            companyId.Value,
            "Executive Structure",
            "EXEC_2026",
            EffectiveFrom: new DateOnly(2026, 1, 1));

        var handler = new CreateSalaryStructureCommandHandler(_salaryStructureRepository, _dbContext, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _salaryStructureRepository.Received(1).AddAsync(Arg.Any<SalaryStructure>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSalaryStructure_ShouldThrowBusinessRuleException_WhenCodeAlreadyExists()
    {
        // Arrange
        var company = Company.Create("Acme", "ACME");
        var companyId = company.Id;
        var existingStructure = SalaryStructure.Create(companyId, "Existing Structure", "EXEC_2026", new DateOnly(2026, 1, 1));

        _dbContext.Companies.Returns(new List<Company> { company }.AsQueryable());
        _dbContext.SalaryStructures.Returns(new List<SalaryStructure> { existingStructure }.AsQueryable());

        var command = new CreateSalaryStructureCommand(
            companyId.Value,
            "Duplicate Structure",
            "EXEC_2026",
            EffectiveFrom: new DateOnly(2026, 1, 1));

        var handler = new CreateSalaryStructureCommandHandler(_salaryStructureRepository, _dbContext, _unitOfWork);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BusinessRuleViolationException>();
        ex.Which.ErrorCode.Should().Be("DUPLICATE_STRUCTURE_CODE");
    }

    [Fact]
    public async Task AddComponentToStructure_ShouldAddComponent_WhenFormulaIsValid()
    {
        // Arrange
        var companyId = CompanyId.New();
        var structure = SalaryStructure.Create(companyId, "Exec Structure", "EXEC_2026", new DateOnly(2026, 1, 1));
        var component = SalaryComponent.Create(companyId, "House Rent", "HOUSE_RENT", ComponentType.Earning, CalculationMethod.PercentageOfBase);

        _salaryStructureRepository.GetByIdAsync(structure.Id, Arg.Any<CancellationToken>()).Returns(structure);
        _salaryStructureRepository.GetByIdWithComponentsAsync(structure.Id, Arg.Any<CancellationToken>()).Returns(structure);
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent> { component }.AsQueryable());

        _formulaEvaluator.Validate(Arg.Any<string>(), Arg.Any<HashSet<string>>())
            .Returns(new List<string>());

        var command = new AddComponentToStructureCommand(
            structure.Id.Value,
            companyId.Value,
            component.Id.Value,
            FormulaExpression: "BASE * 0.45",
            Sequence: 1);

        var handler = new AddComponentToStructureCommandHandler(
            _salaryStructureRepository, _formulaEvaluator, _dbContext, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        structure.Components.Should().HaveCount(1);
        _salaryStructureRepository.Received(1).Update(structure);
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddComponentToStructure_ShouldThrowBusinessRuleException_WhenFormulaIsInvalid()
    {
        // Arrange
        var companyId = CompanyId.New();
        var structure = SalaryStructure.Create(companyId, "Exec Structure", "EXEC_2026", new DateOnly(2026, 1, 1));
        var component = SalaryComponent.Create(companyId, "House Rent", "HOUSE_RENT", ComponentType.Earning, CalculationMethod.PercentageOfBase);

        _salaryStructureRepository.GetByIdAsync(structure.Id, Arg.Any<CancellationToken>()).Returns(structure);
        _salaryStructureRepository.GetByIdWithComponentsAsync(structure.Id, Arg.Any<CancellationToken>()).Returns(structure);
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent> { component }.AsQueryable());

        _formulaEvaluator.Validate(Arg.Any<string>(), Arg.Any<HashSet<string>>())
            .Returns(new List<string> { "Undefined variable 'UNDEFINED_VAR'" });

        var command = new AddComponentToStructureCommand(
            structure.Id.Value,
            companyId.Value,
            component.Id.Value,
            FormulaExpression: "BASE * UNDEFINED_VAR",
            Sequence: 1);

        var handler = new AddComponentToStructureCommandHandler(
            _salaryStructureRepository, _formulaEvaluator, _dbContext, _unitOfWork);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BusinessRuleViolationException>();
        ex.Which.ErrorCode.Should().Be("INVALID_FORMULA");
    }

    [Fact]
    public async Task ValidateFormulaQuery_ShouldReturnValidResult_WhenExpressionIsCorrect()
    {
        // Arrange
        _formulaEvaluator.Validate("BASE * 0.45", Arg.Any<HashSet<string>>())
            .Returns(new List<string>());

        _formulaEvaluator.Evaluate("BASE * 0.45", Arg.Any<FormulaContext>())
            .Returns(22500m);

        var query = new ValidateFormulaQuery("BASE * 0.45");
        var handler = new ValidateFormulaQueryHandler(_formulaEvaluator);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsValid.Should().BeTrue();
        result.DryRunSampleValue.Should().Be(22500m);
    }
}

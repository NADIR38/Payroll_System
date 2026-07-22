using FluentAssertions;
using NSubstitute;
using PayrollMS.Application.Features.SalaryComponents.Commands.CreateSalaryComponent;
using PayrollMS.Application.Features.SalaryComponents.Commands.DeactivateSalaryComponent;
using PayrollMS.Application.Features.SalaryComponents.Commands.SeedPredefinedSalaryComponents;
using PayrollMS.Application.Features.SalaryComponents.Commands.UpdateSalaryComponent;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using Xunit;

namespace PayrollMS.Application.UnitTests;

public class SalaryComponentApplicationTests
{
    private readonly ISalaryComponentRepository _salaryComponentRepository = Substitute.For<ISalaryComponentRepository>();
    private readonly IAppDbContext _dbContext = Substitute.For<IAppDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateSalaryComponent_ShouldReturnNewId_WhenValid()
    {
        // Arrange
        var company = Company.Create("Acme", "ACME");
        var companyId = company.Id;
        var companyList = new List<Company> { company };

        _dbContext.Companies.Returns(companyList.AsQueryable());
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent>().AsQueryable());

        var command = new CreateSalaryComponentCommand(
            companyId.Value,
            "Basic Salary",
            "BASIC",
            "Earning",
            "Fixed",
            DefaultValue: 50000);

        var handler = new CreateSalaryComponentCommandHandler(_salaryComponentRepository, _dbContext, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _salaryComponentRepository.Received(1).AddAsync(Arg.Any<SalaryComponent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSalaryComponent_ShouldThrowNotFoundException_WhenCompanyDoesNotExist()
    {
        // Arrange
        _dbContext.Companies.Returns(new List<Company>().AsQueryable());
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent>().AsQueryable());

        var command = new CreateSalaryComponentCommand(
            Guid.NewGuid(),
            "Basic Salary",
            "BASIC",
            "Earning",
            "Fixed");

        var handler = new CreateSalaryComponentCommandHandler(_salaryComponentRepository, _dbContext, _unitOfWork);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateSalaryComponent_ShouldThrowBusinessRuleException_WhenCodeAlreadyExists()
    {
        // Arrange
        var company = Company.Create("Acme", "ACME");
        var companyId = company.Id;
        var existingComponent = SalaryComponent.Create(companyId, "Basic Salary", "BASIC", ComponentType.Earning, CalculationMethod.Fixed);
        
        _dbContext.Companies.Returns(new List<Company> { company }.AsQueryable());
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent> { existingComponent }.AsQueryable());

        var command = new CreateSalaryComponentCommand(
            companyId.Value,
            "Basic Salary Duplicate",
            "BASIC",
            "Earning",
            "Fixed");

        var handler = new CreateSalaryComponentCommandHandler(_salaryComponentRepository, _dbContext, _unitOfWork);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BusinessRuleViolationException>();
        ex.Which.ErrorCode.Should().Be("DUPLICATE_COMPONENT_CODE");
    }

    [Fact]
    public async Task UpdateSalaryComponent_ShouldModifyComponent_WhenValid()
    {
        // Arrange
        var companyId = CompanyId.New();
        var component = SalaryComponent.Create(companyId, "Basic", "BASIC", ComponentType.Earning, CalculationMethod.Fixed);
        _salaryComponentRepository.GetByIdAsync(component.Id, Arg.Any<CancellationToken>()).Returns(component);

        var command = new UpdateSalaryComponentCommand(
            component.Id.Value,
            companyId.Value,
            "Basic Salary Updated",
            "Earning",
            "Fixed",
            DefaultValue: 60000,
            IsTaxable: true,
            IsRecurring: true,
            IsOptional: false,
            Description: "Updated description");

        var handler = new UpdateSalaryComponentCommandHandler(_salaryComponentRepository, _unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        component.Name.Should().Be("Basic Salary Updated");
        component.DefaultValue.Should().Be(60000);
        _salaryComponentRepository.Received(1).Update(component);
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateSalaryComponent_ShouldDeactivate_WhenValid()
    {
        // Arrange
        var companyId = CompanyId.New();
        var component = SalaryComponent.Create(companyId, "Basic", "BASIC", ComponentType.Earning, CalculationMethod.Fixed);
        _salaryComponentRepository.GetByIdAsync(component.Id, Arg.Any<CancellationToken>()).Returns(component);

        var command = new DeactivateSalaryComponentCommand(component.Id.Value, companyId.Value);
        var handler = new DeactivateSalaryComponentCommandHandler(_salaryComponentRepository, _unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        component.IsActive.Should().BeFalse();
        _salaryComponentRepository.Received(1).Update(component);
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SeedPredefinedSalaryComponents_ShouldSeed14Components_WhenNoneExist()
    {
        // Arrange
        var company = Company.Create("Acme", "ACME");
        var companyId = company.Id;
        _dbContext.Companies.Returns(new List<Company> { company }.AsQueryable());
        _dbContext.SalaryComponents.Returns(new List<SalaryComponent>().AsQueryable());

        var command = new SeedPredefinedSalaryComponentsCommand(companyId.Value);
        var handler = new SeedPredefinedSalaryComponentsCommandHandler(_salaryComponentRepository, _dbContext, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalPredefinedCount.Should().Be(14);
        result.NewlySeededCount.Should().Be(14);
        await _salaryComponentRepository.Received(14).AddAsync(Arg.Any<SalaryComponent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}

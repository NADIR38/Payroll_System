using Xunit;
using FluentAssertions;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.UnitTests;

public class SalaryComponentDomainTests
{
    [Fact]
    public void Create_SalaryComponent_WithValidParameters_ShouldSucceed_AndRaiseCreatedEvent()
    {
        // Arrange
        var companyId = CompanyId.New();

        // Act
        var component = SalaryComponent.Create(
            companyId,
            "Basic Salary",
            "BASIC",
            ComponentType.Earning,
            CalculationMethod.Fixed,
            defaultValue: null,
            isTaxable: true,
            isRecurring: true,
            isOptional: false,
            description: "Base salary component");

        // Assert
        component.Should().NotBeNull();
        component.Id.Should().NotBe(SalaryComponentId.Empty);
        component.CompanyId.Should().Be(companyId);
        component.Name.Should().Be("Basic Salary");
        component.Code.Should().Be("BASIC");
        component.Type.Should().Be(ComponentType.Earning);
        component.CalculationMethod.Should().Be(CalculationMethod.Fixed);
        component.IsActive.Should().BeTrue();

        component.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SalaryComponentCreatedEvent>();
    }

    [Theory]
    [InlineData("", "BASIC", "NameRequired")]
    [InlineData("Basic Salary", "", "CodeRequired")]
    public void Create_SalaryComponent_WithEmptyRequiredFields_ShouldThrowException(
        string name, string code, string expectedErrorCode)
    {
        // Act
        Action act = () => SalaryComponent.Create(
            CompanyId.New(), name, code, ComponentType.Earning, CalculationMethod.Fixed);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be(expectedErrorCode);
    }

    [Fact]
    public void Update_SalaryComponent_ShouldUpdateFields_AndRaiseUpdatedEvent()
    {
        // Arrange
        var component = SalaryComponent.Create(
            CompanyId.New(), "House Rent", "HOUSE_RENT", ComponentType.Earning, CalculationMethod.PercentageOfBase);

        component.ClearDomainEvents();

        // Act
        component.Update(
            "House Rent Allowance",
            ComponentType.Earning,
            CalculationMethod.FormulaExpression,
            defaultValue: 40,
            isTaxable: true,
            isRecurring: true,
            isOptional: false,
            description: "40% of base salary");

        // Assert
        component.Name.Should().Be("House Rent Allowance");
        component.CalculationMethod.Should().Be(CalculationMethod.FormulaExpression);
        component.DefaultValue.Should().Be(40);

        component.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SalaryComponentUpdatedEvent>();
    }

    [Fact]
    public void Deactivate_SalaryComponent_ShouldSetIsActiveFalse_AndRaiseDeactivatedEvent()
    {
        // Arrange
        var component = SalaryComponent.Create(
            CompanyId.New(), "Transport", "TRANSPORT", ComponentType.Earning, CalculationMethod.Fixed);

        component.ClearDomainEvents();

        // Act
        component.Deactivate();

        // Assert
        component.IsActive.Should().BeFalse();
        component.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SalaryComponentDeactivatedEvent>();
    }
}

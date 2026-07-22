using Xunit;
using FluentAssertions;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.UnitTests;

public class SalaryStructureDomainTests
{
    [Fact]
    public void Create_SalaryStructure_WithValidParameters_ShouldSucceed_AndRaiseCreatedEvent()
    {
        // Arrange
        var companyId = CompanyId.New();
        var effectiveFrom = new DateOnly(2025, 1, 1);

        // Act
        var structure = SalaryStructure.Create(
            companyId,
            "Teacher Salary Structure",
            "TEACHER_V1",
            effectiveFrom,
            description: "Standard structure for teaching staff");

        // Assert
        structure.Should().NotBeNull();
        structure.Id.Should().NotBe(SalaryStructureId.Empty);
        structure.CompanyId.Should().Be(companyId);
        structure.Name.Should().Be("Teacher Salary Structure");
        structure.Code.Should().Be("TEACHER_V1");
        structure.EffectiveFrom.Should().Be(effectiveFrom);
        structure.IsActive.Should().BeTrue();
        structure.Components.Should().BeEmpty();

        structure.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SalaryStructureCreatedEvent>();
    }

    [Fact]
    public void AddComponent_WithValidParameters_ShouldSucceed_AndRaiseStructureComponentAddedEvent()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        structure.ClearDomainEvents();

        var componentId = SalaryComponentId.New();

        // Act
        var comp = structure.AddComponent(componentId, "BaseSalary", sequence: 1);

        // Assert
        comp.Should().NotBeNull();
        comp.SalaryComponentId.Should().Be(componentId);
        comp.FormulaExpression.Should().Be("BaseSalary");
        comp.Sequence.Should().Be(1);

        structure.Components.Should().ContainSingle();
        structure.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<StructureComponentAddedEvent>();
    }

    [Fact]
    public void AddComponent_WithDuplicateComponent_ShouldThrowException()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        var componentId = SalaryComponentId.New();
        structure.AddComponent(componentId, "BaseSalary", sequence: 1);

        // Act
        Action act = () => structure.AddComponent(componentId, "BaseSalary * 0.4", sequence: 2);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("DuplicateComponent");
    }

    [Fact]
    public void AddComponent_WithDuplicateSequence_ShouldThrowException()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        var comp1Id = SalaryComponentId.New();
        var comp2Id = SalaryComponentId.New();

        structure.AddComponent(comp1Id, "BaseSalary", sequence: 1);

        // Act
        Action act = () => structure.AddComponent(comp2Id, "BaseSalary * 0.4", sequence: 1);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
           .And.ErrorCode.Should().Be("DuplicateSequence");
    }

    [Fact]
    public void SetAllowanceRule_ShouldCreateAllowanceRule_AndRaiseEvent()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        var comp = structure.AddComponent(SalaryComponentId.New(), "WorkingDays * 250", sequence: 1);
        structure.ClearDomainEvents();

        // Act
        var rule = structure.SetAllowanceRule(comp.Id, AllowanceApplicationMode.WorkingDaysOnly, description: "Travel allowance per day");

        // Assert
        rule.Should().NotBeNull();
        rule.ApplicationMode.Should().Be(AllowanceApplicationMode.WorkingDaysOnly);
        comp.AllowanceRule.Should().Be(rule);

        structure.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AllowanceRuleSetEvent>();
    }

    [Fact]
    public void SetDeductionRule_ShouldCreateDeductionRule_AndRaiseEvent()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        var comp = structure.AddComponent(SalaryComponentId.New(), "LateDays * 200", sequence: 1);
        structure.ClearDomainEvents();

        // Act
        var rule = structure.SetDeductionRule(
            comp.Id,
            DeductionType.LateBased,
            isOptIn: true,
            gracePeriodMinutes: 15,
            deductionFormula: "If(LateMinutes > 15, LateDays * 200, 0)");

        // Assert
        rule.Should().NotBeNull();
        rule.DeductionType.Should().Be(DeductionType.LateBased);
        rule.IsOptIn.Should().BeTrue();
        rule.GracePeriodMinutes.Should().Be(15);
        comp.DeductionRule.Should().Be(rule);

        structure.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DeductionRuleSetEvent>();
    }

    [Fact]
    public void RemoveComponent_ShouldDeactivateComponent_AndRaiseEvent()
    {
        // Arrange
        var structure = SalaryStructure.Create(
            CompanyId.New(), "Standard Structure", "STD_V1", new DateOnly(2025, 1, 1));

        var comp = structure.AddComponent(SalaryComponentId.New(), "BaseSalary", sequence: 1);
        structure.ClearDomainEvents();

        // Act
        structure.RemoveComponent(comp.Id);

        // Assert
        comp.IsActive.Should().BeFalse();

        structure.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<StructureComponentRemovedEvent>();
    }
}

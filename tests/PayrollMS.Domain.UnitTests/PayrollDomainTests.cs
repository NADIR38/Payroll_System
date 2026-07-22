using Xunit;
using FluentAssertions;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Payroll;
using PayrollMS.Domain.Entities.Approval;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.UnitTests;

public class PayrollDomainTests
{
    [Fact]
    public void Create_AttendanceSummary_WithValidInputs_ShouldSucceed_AndRaiseEvent()
    {
        var companyId = CompanyId.New();
        var summary = AttendanceSummary.Create(
            companyId, "EMP-1001", 2025, 7, 22, 1, 3, 45, 2.5m, 0, 1, 8, "COMP1-EMP1001-2025-07", "ERP");

        summary.Should().NotBeNull();
        summary.WorkingDays.Should().Be(22);
        summary.AbsentDays.Should().Be(1);
        summary.LateMinutes.Should().Be(45);
        summary.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AttendanceSyncedEvent>();
    }

    [Fact]
    public void Create_PayrollRun_ShouldInitializeInDraftStatus()
    {
        var companyId = CompanyId.New();
        var finYearId = FinancialYearId.New();

        var run = PayrollRun.Create(
            companyId, finYearId, 2025, 7, PayrollRunType.Regular, "Admin");

        run.Should().NotBeNull();
        run.Status.Should().Be(PayrollRunStatus.Draft);
        run.RunVersion.Should().Be(1);
        run.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<PayrollRunCreatedEvent>();
    }

    [Fact]
    public void TransitionTo_InvalidStatus_ShouldThrowInvalidPayrollStateTransitionException()
    {
        var companyId = CompanyId.New();
        var finYearId = FinancialYearId.New();
        var run = PayrollRun.Create(companyId, finYearId, 2025, 7, PayrollRunType.Regular, "Admin");

        Action act = () => run.MarkApproved("Admin");

        act.Should().Throw<InvalidPayrollStateTransitionException>();
    }

    [Fact]
    public void Create_ApprovalWorkflowTemplate_AndAddStep_ShouldSucceed()
    {
        var companyId = CompanyId.New();
        var template = ApprovalWorkflowTemplate.Create(companyId, "Standard 2-Step", true, "Admin");

        var step1 = template.AddStep(1, "HR Review", "HRManager", false, 24);
        var step2 = template.AddStep(2, "Finance Approval", "FinanceManager", false, 48);

        template.Steps.Should().HaveCount(2);
        step1.StepOrder.Should().Be(1);
        step2.RequiredRole.Should().Be("FinanceManager");
    }

    [Fact]
    public void OverrideComponent_WhenRunIsApproved_ShouldThrowPayrollFrozenException()
    {
        var runId = PayrollRunId.New();
        var companyId = CompanyId.New();
        var componentId = SalaryComponentId.New();

        var comp = PayrollEntryComponent.Create(
            PayrollEntryId.New(), companyId, componentId, "BASIC", "Basic Salary", ComponentType.Earning, "BaseSalary", 50000m);

        var entry = PayrollEntry.CreateCalculated(
            runId, companyId, "EMP-1", "EMP1", "John", "Dept", "Desg", "Bank", "IBAN", 50000m, 22, 0, 0, 50000m, 0m, new[] { comp });

        Action act = () => entry.OverrideComponent(comp.Id, 55000m, "HR", "Adjustment", PayrollRunStatus.Approved);

        act.Should().Throw<PayrollFrozenException>();
    }
}

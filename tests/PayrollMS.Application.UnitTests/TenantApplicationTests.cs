using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using PayrollMS.Application.Common.Behaviors;
using PayrollMS.Application.Features.Tenant;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Tenant;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using Xunit;
using ValidationException = PayrollMS.Application.Common.Exceptions.ValidationException;

namespace PayrollMS.Application.UnitTests;

public class TenantApplicationTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantApplicationTests()
    {
        _companyRepository = Substitute.For<ICompanyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task ValidationBehavior_ShouldThrowValidationException_WhenCommandIsInvalid()
    {
        // Arrange
        var command = new CreateCompanyCommand(
            "Acme Corp",
            "ACME123",
            null,
            null,
            "invalid-email",
            "+1234567890"
        );

        var validator = new CreateCompanyCommandValidator();
        var validators = new List<IValidator<CreateCompanyCommand>> { validator };
        var behavior = new ValidationBehavior<CreateCompanyCommand, Guid>(validators);

        RequestHandlerDelegate<Guid> next = (cancellationToken) => Task.FromResult(Guid.NewGuid());

        // Act
        Func<Task> act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("ContactEmail");
    }

    [Fact]
    public async Task CreateCompanyCommandHandler_ShouldThrowDuplicateException_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new CreateCompanyCommand("Acme Corp", "ACME", null, null, null, null);
        var existingCompany = Company.Create("Existing Acme", "ACME");

        _companyRepository.GetByCodeAsync("ACME", Arg.Any<CancellationToken>())
            .Returns(existingCompany);

        var handler = new CreateCompanyCommandHandler(_companyRepository, _unitOfWork);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<BusinessRuleViolationException>();
        exception.Which.ErrorCode.Should().Be("DUPLICATE_COMPANY_CODE");
    }

    [Fact]
    public async Task UpdateCompanyDetailsCommandHandler_ShouldAcquirePessimisticLock_AndCommitTransaction()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var command = new UpdateCompanyDetailsCommand(
            companyId,
            "Acme Refactored",
            null,
            null,
            null,
            null
        );

        var company = Company.Create("Acme Inc", "ACME");
        _companyRepository.GetByIdForUpdateAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        var handler = new UpdateCompanyDetailsCommandHandler(_companyRepository, _unitOfWork);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify pessimistic lock query was executed
        await _companyRepository.Received(1).GetByIdForUpdateAsync(
            Arg.Is<CompanyId>(id => id.Value == companyId),
            Arg.Any<CancellationToken>());

        // Verify transaction block was initialized and committed
        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}

using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Employee.Commands.AddEmployeeBankAccount;

public class AddEmployeeBankAccountCommandHandler : IRequestHandler<AddEmployeeBankAccountCommand, Guid>
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddEmployeeBankAccountCommandHandler(
        IEmployeePayrollProfileRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddEmployeeBankAccountCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var profileId = new EmployeePayrollProfileId(request.EmployeePayrollProfileId);
        var profile = await _employeeRepository.GetByIdForUpdateAsync(profileId, cancellationToken);
        if (profile == null)
            throw new NotFoundException($"Employee profile with ID '{request.EmployeePayrollProfileId}' was not found.");

        var account = profile.AddBankAccount(
            request.BankName,
            request.AccountTitle,
            request.AccountNumber,
            request.IBAN,
            request.BranchCode,
            request.IsPrimary);

        _employeeRepository.Update(profile);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return account.Id.Value;
    }
}

using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Employee.Commands.SetPrimaryBankAccount;

public class SetPrimaryBankAccountCommandHandler : IRequestHandler<SetPrimaryBankAccountCommand>
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrimaryBankAccountCommandHandler(
        IEmployeePayrollProfileRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SetPrimaryBankAccountCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var profileId = new EmployeePayrollProfileId(request.EmployeePayrollProfileId);
        var profile = await _employeeRepository.GetByIdForUpdateAsync(profileId, cancellationToken);
        if (profile == null)
            throw new NotFoundException($"Employee profile with ID '{request.EmployeePayrollProfileId}' was not found.");

        profile.SetPrimaryBankAccount(new EmployeeBankAccountId(request.BankAccountId));

        _employeeRepository.Update(profile);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

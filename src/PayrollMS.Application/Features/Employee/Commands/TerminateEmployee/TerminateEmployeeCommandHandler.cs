using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Employee.Commands.TerminateEmployee;

public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand>
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateEmployeeCommandHandler(
        IEmployeePayrollProfileRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var profileId = new EmployeePayrollProfileId(request.Id);
        var profile = await _employeeRepository.GetByIdForUpdateAsync(profileId, cancellationToken);
        if (profile == null)
            throw new NotFoundException($"Employee profile with ID '{request.Id}' was not found.");

        profile.Terminate(request.ChangedBy);

        _employeeRepository.Update(profile);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

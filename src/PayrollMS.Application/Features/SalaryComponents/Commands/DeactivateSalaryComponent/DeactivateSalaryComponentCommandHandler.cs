using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.DeactivateSalaryComponent;

public class DeactivateSalaryComponentCommandHandler : IRequestHandler<DeactivateSalaryComponentCommand>
{
    private readonly ISalaryComponentRepository _salaryComponentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSalaryComponentCommandHandler(
        ISalaryComponentRepository salaryComponentRepository,
        IUnitOfWork unitOfWork)
    {
        _salaryComponentRepository = salaryComponentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateSalaryComponentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var componentId = new SalaryComponentId(request.ComponentId);
        var companyId = new CompanyId(request.CompanyId);

        var component = await _salaryComponentRepository.GetByIdAsync(componentId, cancellationToken);
        if (component == null || component.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary component with ID '{request.ComponentId}' was not found for this company.");
        }

        component.Deactivate();

        _salaryComponentRepository.Update(component);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

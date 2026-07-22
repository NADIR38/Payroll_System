using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;
using  PayrollMS.Domain.Interfaces.Services;
namespace PayrollMS.Application.Features.SalaryComponents.Commands.UpdateSalaryComponent;

public class UpdateSalaryComponentCommandHandler : IRequestHandler<UpdateSalaryComponentCommand>
{
    private readonly ISalaryComponentRepository _salaryComponentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSalaryComponentCommandHandler(
        ISalaryComponentRepository salaryComponentRepository,
        IUnitOfWork unitOfWork)
    {
        _salaryComponentRepository = salaryComponentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSalaryComponentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var componentId = new SalaryComponentId(request.ComponentId);
        var companyId = new CompanyId(request.CompanyId);

        var component = await _salaryComponentRepository.GetByIdAsync(componentId, cancellationToken);
        if (component == null || component.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary component with ID '{request.ComponentId}' was not found for this company.");
        }

        var type = Enum.Parse<ComponentType>(request.Type, ignoreCase: true);
        var method = Enum.Parse<CalculationMethod>(request.CalculationMethod, ignoreCase: true);

        component.Update(
            request.Name,
            type,
            method,
            request.DefaultValue,
            request.IsTaxable,
            request.IsRecurring,
            request.IsOptional,
            request.Description);

        _salaryComponentRepository.Update(component);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

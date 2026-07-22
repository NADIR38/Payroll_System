using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.RemoveComponentFromStructure;

public class RemoveComponentFromStructureCommandHandler : IRequestHandler<RemoveComponentFromStructureCommand>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveComponentFromStructureCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveComponentFromStructureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var structureId = new SalaryStructureId(request.SalaryStructureId);
        var companyId = new CompanyId(request.CompanyId);
        var structureComponentId = new SalaryStructureComponentId(request.StructureComponentId);

        var structure = await _salaryStructureRepository.GetByIdWithComponentsAsync(structureId, cancellationToken);
        if (structure == null || structure.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary structure with ID '{request.SalaryStructureId}' was not found for this company.");
        }

        structure.RemoveComponent(structureComponentId);

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

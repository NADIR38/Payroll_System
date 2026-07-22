using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.DeactivateSalaryStructure;

public class DeactivateSalaryStructureCommandHandler : IRequestHandler<DeactivateSalaryStructureCommand>
{
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSalaryStructureCommandHandler(
        ISalaryStructureRepository salaryStructureRepository,
        IUnitOfWork unitOfWork)
    {
        _salaryStructureRepository = salaryStructureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var structureId = new SalaryStructureId(request.SalaryStructureId);
        var companyId = new CompanyId(request.CompanyId);

        var structure = await _salaryStructureRepository.GetByIdAsync(structureId, cancellationToken);
        if (structure == null || structure.CompanyId != companyId)
        {
            throw new NotFoundException($"Salary structure with ID '{request.SalaryStructureId}' was not found for this company.");
        }

        structure.Deactivate();

        _salaryStructureRepository.Update(structure);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

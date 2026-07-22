using MediatR;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Approval;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Approvals.Commands.CreateWorkflowTemplate;

public sealed record CreateWorkflowStepDto(
    int StepOrder,
    string StepName,
    string RequiredRole,
    bool IsOptional = false,
    int? SLAHours = null);

public sealed record CreateWorkflowTemplateCommand(
    Guid CompanyId,
    string Name,
    bool IsDefault,
    IReadOnlyList<CreateWorkflowStepDto> Steps,
    string CreatedBy = "Admin") : IRequest<ApprovalWorkflowTemplateResponse>;

public sealed class CreateWorkflowTemplateCommandHandler : IRequestHandler<CreateWorkflowTemplateCommand, ApprovalWorkflowTemplateResponse>
{
    private readonly IApprovalWorkflowTemplateRepository _templateRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkflowTemplateCommandHandler(
        IApprovalWorkflowTemplateRepository templateRepo,
        IUnitOfWork unitOfWork)
    {
        _templateRepo = templateRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApprovalWorkflowTemplateResponse> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var companyId = new CompanyId(request.CompanyId);

        if (request.IsDefault)
        {
            await _templateRepo.ClearDefaultsAsync(companyId, cancellationToken);
        }

        var template = ApprovalWorkflowTemplate.Create(
            companyId, request.Name, request.IsDefault, request.CreatedBy);

        foreach (var stepDto in request.Steps.OrderBy(s => s.StepOrder))
        {
            template.AddStep(
                stepDto.StepOrder,
                stepDto.StepName,
                stepDto.RequiredRole,
                stepDto.IsOptional,
                stepDto.SLAHours);
        }

        await _templateRepo.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApprovalWorkflowTemplateResponse(
            template.Id.Value,
            template.CompanyId.Value,
            template.Name,
            template.IsDefault,
            template.IsActive,
            template.Steps.Select(s => new ApprovalWorkflowStepResponse(
                s.Id.Value,
                s.WorkflowTemplateId.Value,
                s.StepOrder,
                s.StepName,
                s.RequiredRole,
                s.IsOptional,
                s.SLAHours)).ToList());
    }
}

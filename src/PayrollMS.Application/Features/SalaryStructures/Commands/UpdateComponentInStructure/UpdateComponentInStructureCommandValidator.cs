using FluentValidation;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.UpdateComponentInStructure;

public class UpdateComponentInStructureCommandValidator : AbstractValidator<UpdateComponentInStructureCommand>
{
    public UpdateComponentInStructureCommandValidator()
    {
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.StructureComponentId).NotEmpty();
        RuleFor(x => x.Sequence).GreaterThan(0).WithMessage("Sequence must be a positive integer greater than zero.");
    }
}

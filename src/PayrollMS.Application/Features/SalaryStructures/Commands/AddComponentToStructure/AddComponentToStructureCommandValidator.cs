using FluentValidation;

namespace PayrollMS.Application.Features.SalaryStructures.Commands.AddComponentToStructure;

public class AddComponentToStructureCommandValidator : AbstractValidator<AddComponentToStructureCommand>
{
    public AddComponentToStructureCommandValidator()
    {
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalaryComponentId).NotEmpty();
        RuleFor(x => x.Sequence).GreaterThan(0).WithMessage("Sequence must be a positive integer greater than zero.");
    }
}

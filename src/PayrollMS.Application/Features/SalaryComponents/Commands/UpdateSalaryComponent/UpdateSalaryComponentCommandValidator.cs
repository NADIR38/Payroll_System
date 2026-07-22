using FluentValidation;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.UpdateSalaryComponent;

public class UpdateSalaryComponentCommandValidator : AbstractValidator<UpdateSalaryComponentCommand>
{
    public UpdateSalaryComponentCommandValidator()
    {
        RuleFor(x => x.ComponentId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeAValidComponentType)
            .WithMessage("Invalid component type. Allowed: Earning, Deduction, EmployerContribution.");

        RuleFor(x => x.CalculationMethod)
            .NotEmpty()
            .Must(BeAValidCalculationMethod)
            .WithMessage("Invalid calculation method. Allowed: Fixed, PercentageOfBase, FormulaExpression, PerWorkingDay, PerHour.");
    }

    private static bool BeAValidComponentType(string type) =>
        Enum.TryParse<ComponentType>(type, true, out _);

    private static bool BeAValidCalculationMethod(string method) =>
        Enum.TryParse<CalculationMethod>(method, true, out _);
}

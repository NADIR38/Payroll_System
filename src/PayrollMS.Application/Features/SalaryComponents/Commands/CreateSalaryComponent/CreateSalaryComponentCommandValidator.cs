using FluentValidation;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Application.Features.SalaryComponents.Commands.CreateSalaryComponent;

public class CreateSalaryComponentCommandValidator : AbstractValidator<CreateSalaryComponentCommand>
{
    public CreateSalaryComponentCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^[A-Z0-9_-]{2,50}$")
            .WithMessage("Code must be uppercase alphanumeric (e.g. BASIC, HOUSE_RENT).");

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

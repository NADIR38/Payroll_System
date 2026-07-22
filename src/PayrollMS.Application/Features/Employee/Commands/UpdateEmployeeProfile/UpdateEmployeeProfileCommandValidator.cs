using FluentValidation;

namespace PayrollMS.Application.Features.Employee.Commands.UpdateEmployeeProfile;

public class UpdateEmployeeProfileCommandValidator : AbstractValidator<UpdateEmployeeProfileCommand>
{
    public UpdateEmployeeProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DesignationId).NotEmpty();
        RuleFor(x => x.SalaryStructureId).NotEmpty();
        RuleFor(x => x.BaseSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ChangedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ChangeReason).NotEmpty().MaximumLength(500);
    }
}

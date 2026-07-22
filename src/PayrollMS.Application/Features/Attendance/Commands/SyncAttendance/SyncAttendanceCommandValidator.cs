using FluentValidation;

namespace PayrollMS.Application.Features.Attendance.Commands.SyncAttendance;

public sealed class SyncAttendanceCommandValidator : AbstractValidator<SyncAttendanceCommand>
{
    public SyncAttendanceCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId is required.");

        RuleFor(x => x.ExternalEmployeeId)
            .NotEmpty().WithMessage("ExternalEmployeeId is required.");

        RuleFor(x => x.PeriodYear)
            .InclusiveBetween(2000, 2100).WithMessage("PeriodYear is invalid.");

        RuleFor(x => x.PeriodMonth)
            .InclusiveBetween(1, 12).WithMessage("PeriodMonth must be between 1 and 12.");

        RuleFor(x => x.WorkingDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AbsentDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LateDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LateMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HalfDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Holidays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Weekends).GreaterThanOrEqualTo(0);

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey is required.");
    }
}

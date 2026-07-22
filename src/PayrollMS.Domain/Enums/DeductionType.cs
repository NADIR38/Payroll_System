namespace PayrollMS.Domain.Enums;

/// <summary>
/// The category of a deduction, determining how it is sourced and calculated.
/// PRD §12.3
/// </summary>
public enum DeductionType
{
    /// <summary>Per absent day deduction.</summary>
    AttendanceBased = 0,

    /// <summary>Per late day or late minute deduction.</summary>
    LateBased = 1,

    /// <summary>Per half-day deduction (50% of daily rate).</summary>
    HalfDayBased = 2,

    /// <summary>Automatic installment deduction for an active employee loan.</summary>
    LoanRecovery = 3,

    /// <summary>Advance salary repayment installment.</summary>
    AdvanceRecovery = 4,

    /// <summary>Percentage of basic salary contributed to provident fund (future).</summary>
    ProvidentFund = 5,

    /// <summary>One-time ad-hoc manual deduction entered by HR.</summary>
    Manual = 6
}

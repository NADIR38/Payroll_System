using PayrollMS.Domain.Common;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Employee;

/// <summary>
/// Bank account for salary disbursement. An employee can have multiple accounts;
/// exactly one should be primary at any time — PRD §7.2
/// </summary>
public sealed class EmployeeBankAccount : BaseEntity<EmployeeBankAccountId>
{
    private EmployeeBankAccount() { }

    public EmployeePayrollProfileId EmployeePayrollProfileId { get; private set; }
    public CompanyId CompanyId { get; private set; }

    public string BankName { get; private set; } = null!;
    public string AccountTitle { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;

    /// <summary>IBAN — primary identifier for bank transfer records.</summary>
    public string IBAN { get; private set; } = null!;

    public string? BranchCode { get; private set; }

    /// <summary>Only one account should be primary per employee. Enforced by the aggregate.</summary>
    public bool IsPrimary { get; private set; }

    public bool IsActive { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    internal static EmployeeBankAccount Create(
        EmployeePayrollProfileId profileId,
        CompanyId companyId,
        string bankName,
        string accountTitle,
        string accountNumber,
        string iban,
        string? branchCode,
        bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            throw new BusinessRuleViolationException("BankNameRequired", "Bank name is required.");

        if (string.IsNullOrWhiteSpace(accountTitle))
            throw new BusinessRuleViolationException("AccountTitleRequired", "Account title is required.");

        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new BusinessRuleViolationException("AccountNumberRequired", "Account number is required.");

        if (string.IsNullOrWhiteSpace(iban))
            throw new BusinessRuleViolationException("IBANRequired", "IBAN is required.");

        return new EmployeeBankAccount
        {
            Id = EmployeeBankAccountId.New(),
            EmployeePayrollProfileId = profileId,
            CompanyId = companyId,
            BankName = bankName.Trim(),
            AccountTitle = accountTitle.Trim(),
            AccountNumber = accountNumber.Trim(),
            IBAN = iban.Trim().ToUpperInvariant(),
            BranchCode = branchCode?.Trim(),
            IsPrimary = isPrimary,
            IsActive = true
        };
    }

    // ── Domain methods ────────────────────────────────────────────────────────

    public void Update(
        string bankName,
        string accountTitle,
        string accountNumber,
        string iban,
        string? branchCode)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            throw new BusinessRuleViolationException("BankNameRequired", "Bank name is required.");

        if (string.IsNullOrWhiteSpace(iban))
            throw new BusinessRuleViolationException("IBANRequired", "IBAN is required.");

        BankName = bankName.Trim();
        AccountTitle = accountTitle.Trim();
        AccountNumber = accountNumber.Trim();
        IBAN = iban.Trim().ToUpperInvariant();
        BranchCode = branchCode?.Trim();

        SetUpdatedAt();
    }

    /// <summary>Called by the aggregate when another account is set as primary.</summary>
    internal void ClearPrimary()
    {
        IsPrimary = false;
    }

    /// <summary>Called by the aggregate after clearing primary on all siblings.</summary>
    internal void SetAsPrimary()
    {
        IsPrimary = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        if (IsPrimary)
            throw new BusinessRuleViolationException(
                "CannotDeactivatePrimary",
                "Cannot deactivate the primary bank account. Set another account as primary first.");

        IsActive = false;
        SetUpdatedAt();
    }
}

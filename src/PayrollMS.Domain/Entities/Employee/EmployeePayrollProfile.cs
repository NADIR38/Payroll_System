using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Events;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Domain.Entities.Employee;

/// <summary>
/// Payroll-relevant slice of an employee record, synced from an external ERP.
/// Never owns the authoritative HR master data — that stays in the ERP.
///
/// Upsert key: (ExternalEmployeeId + CompanyId) — PRD §7.4
/// Every update writes a history row — PRD §7.4
/// Payroll generation reads the version active on the period start date from history — PRD §7.4
/// </summary>
public sealed class EmployeePayrollProfile : BaseAuditableEntity<EmployeePayrollProfileId>, IAggregateRoot
{
    private readonly List<EmployeePayrollProfileHistory> _history = [];
    private readonly List<EmployeeBankAccount> _bankAccounts = [];

    private EmployeePayrollProfile() { }

    public CompanyId CompanyId { get; private set; }

    /// <summary>
    /// The ERP's own employee identifier. Together with CompanyId this forms the upsert key.
    /// </summary>
    public string ExternalEmployeeId { get; private set; } = null!;

    public string EmployeeCode { get; private set; } = null!;
    public string FullName { get; private set; } = null!;

    public BranchId BranchId { get; private set; }
    public DepartmentId DepartmentId { get; private set; }
    public DesignationId DesignationId { get; private set; }
    public CostCenterId? CostCenterId { get; private set; }

    public SalaryStructureId SalaryStructureId { get; private set; }

    /// <summary>
    /// Plugged in as {BaseSalary} in NCalc formulas. All derived components compute from this.
    /// </summary>
    public decimal BaseSalary { get; private set; }

    /// <summary>
    /// When false and a DeductionRule has IsOptIn = true, attendance deductions are skipped
    /// for this employee — PRD §12.4
    /// </summary>
    public bool AttendanceDeductionOptIn { get; private set; }

    public DateOnly JoiningDate { get; private set; }
    public DateOnly? LeavingDate { get; private set; }
    public EmployeeStatus Status { get; private set; }

    /// <summary>
    /// Start of validity for this profile version. Used by payroll generation to pick
    /// the correct history row for a given period — PRD §7.4
    /// </summary>
    public DateOnly EffectiveFrom { get; private set; }

    public IReadOnlyCollection<EmployeePayrollProfileHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<EmployeeBankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    // ── Factory: Create (first-time sync) ────────────────────────────────────

    /// <summary>
    /// Creates a new profile from an ERP sync. Also writes the first history row.
    /// PRD §7.3 — POST /employees/sync (action = "Created")
    /// </summary>
    public static EmployeePayrollProfile Create(
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string fullName,
        BranchId branchId,
        DepartmentId departmentId,
        DesignationId designationId,
        CostCenterId? costCenterId,
        SalaryStructureId salaryStructureId,
        decimal baseSalary,
        DateOnly joiningDate,
        bool attendanceDeductionOptIn = true,
        string? syncedBy = null)
    {
        ValidateRequiredFields(companyId, externalEmployeeId, employeeCode, fullName, baseSalary);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var profile = new EmployeePayrollProfile
        {
            Id = EmployeePayrollProfileId.New(),
            CompanyId = companyId,
            ExternalEmployeeId = externalEmployeeId.Trim(),
            EmployeeCode = employeeCode.Trim(),
            FullName = fullName.Trim(),
            BranchId = branchId,
            DepartmentId = departmentId,
            DesignationId = designationId,
            CostCenterId = costCenterId,
            SalaryStructureId = salaryStructureId,
            BaseSalary = baseSalary,
            AttendanceDeductionOptIn = attendanceDeductionOptIn,
            JoiningDate = joiningDate,
            Status = EmployeeStatus.Active,
            EffectiveFrom = today
        };

        if (syncedBy is not null)
            profile.SetCreatedBy(syncedBy);

        // Write first history row
        profile._history.Add(EmployeePayrollProfileHistory.CreateInitial(
            profile.Id, companyId, externalEmployeeId, employeeCode, fullName,
            salaryStructureId, baseSalary, today, syncedBy));

        profile.AddDomainEvent(new EmployeeProfileCreatedEvent(
            profile.Id, companyId, externalEmployeeId, employeeCode, fullName));

        return profile;
    }

    // ── Update (re-sync or HR override) ──────────────────────────────────────

    /// <summary>
    /// Updates an existing profile. Writes the old version to history with EffectiveTo = today,
    /// then opens a new history row with EffectiveFrom = today — PRD §7.4
    /// </summary>
    public void Update(
        string fullName,
        BranchId branchId,
        DepartmentId departmentId,
        DesignationId designationId,
        CostCenterId? costCenterId,
        SalaryStructureId salaryStructureId,
        decimal baseSalary,
        DateOnly joiningDate,
        DateOnly? leavingDate,
        bool attendanceDeductionOptIn,
        EmployeeStatus status,
        string changedBy,
        string changeReason)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("NameRequired", "Employee full name is required.");

        if (baseSalary < 0)
            throw new BusinessRuleViolationException("InvalidBaseSalary", "Base salary cannot be negative.");

        if (string.IsNullOrWhiteSpace(changedBy))
            throw new BusinessRuleViolationException("ChangedByRequired", "ChangedBy is required for profile updates.");

        if (string.IsNullOrWhiteSpace(changeReason))
            throw new BusinessRuleViolationException("ChangeReasonRequired", "A reason is required when updating an employee profile.");

        if (Status == EmployeeStatus.Terminated && status != EmployeeStatus.Terminated)
            throw new BusinessRuleViolationException("TerminatedEmployeeCannotBeUpdated", "Cannot modify or reactivate a terminated employee profile directly via update. Use explicit re-hire/activation flows.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Close current open history row
        var openHistory = _history.FirstOrDefault(h => h.EffectiveTo == null);
        openHistory?.Close(today);

        // Open a new history row for the updated values
        _history.Add(EmployeePayrollProfileHistory.Create(
            Id, CompanyId, ExternalEmployeeId, EmployeeCode, fullName,
            salaryStructureId, baseSalary, today, changedBy, changeReason));

        // Apply the updates
        FullName = fullName.Trim();
        BranchId = branchId;
        DepartmentId = departmentId;
        DesignationId = designationId;
        CostCenterId = costCenterId;
        SalaryStructureId = salaryStructureId;
        BaseSalary = baseSalary;
        JoiningDate = joiningDate;
        LeavingDate = leavingDate;
        AttendanceDeductionOptIn = attendanceDeductionOptIn;
        Status = status;
        EffectiveFrom = today;

        SetUpdatedAt();
        SetUpdatedBy(changedBy);

        AddDomainEvent(new EmployeeProfileUpdatedEvent(Id, CompanyId, ExternalEmployeeId, changedBy, changeReason));
    }

    // ── Status transitions ────────────────────────────────────────────────────

    public void Terminate(string changedBy)
    {
        if (Status == EmployeeStatus.Terminated)
            return;

        Status = EmployeeStatus.Terminated;
        LeavingDate = DateOnly.FromDateTime(DateTime.UtcNow);
        SetUpdatedAt();
        AddDomainEvent(new EmployeeProfileStatusChangedEvent(Id, CompanyId, EmployeeStatus.Terminated));
    }

    public void Deactivate()
    {
        if (Status == EmployeeStatus.Terminated)
            throw new BusinessRuleViolationException("InvalidState", "Cannot deactivate a terminated employee.");

        Status = EmployeeStatus.Inactive;
        SetUpdatedAt();
        AddDomainEvent(new EmployeeProfileStatusChangedEvent(Id, CompanyId, EmployeeStatus.Inactive));
    }

    public void Activate()
    {
        if (Status == EmployeeStatus.Terminated)
            throw new BusinessRuleViolationException("InvalidState", "Cannot reactivate a terminated employee.");

        Status = EmployeeStatus.Active;
        SetUpdatedAt();
        AddDomainEvent(new EmployeeProfileStatusChangedEvent(Id, CompanyId, EmployeeStatus.Active));
    }

    // ── Bank account management ───────────────────────────────────────────────

    /// <summary>
    /// Adds a bank account. If IsPrimary, clears primary flag on all existing accounts first.
    /// </summary>
    public EmployeeBankAccount AddBankAccount(
        string bankName,
        string accountTitle,
        string accountNumber,
        string iban,
        string? branchCode,
        bool isPrimary)
    {
        if (isPrimary)
        {
            foreach (var existing in _bankAccounts.Where(a => a.IsActive))
                existing.ClearPrimary();
        }

        var account = EmployeeBankAccount.Create(
            Id, CompanyId, bankName, accountTitle, accountNumber, iban, branchCode, isPrimary);

        _bankAccounts.Add(account);
        AddDomainEvent(new EmployeeBankAccountAddedEvent(account.Id, Id, CompanyId, isPrimary));

        return account;
    }

    /// <summary>Sets an existing bank account as the primary disbursement account.</summary>
    public void SetPrimaryBankAccount(EmployeeBankAccountId accountId)
    {
        var target = _bankAccounts.FirstOrDefault(a => a.Id == accountId && a.IsActive)
            ?? throw new BusinessRuleViolationException("AccountNotFound", "Bank account not found or is inactive.");

        foreach (var account in _bankAccounts.Where(a => a.IsActive))
            account.ClearPrimary();

        target.SetAsPrimary();
        AddDomainEvent(new EmployeeBankAccountSetPrimaryEvent(accountId, Id));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static void ValidateRequiredFields(
        CompanyId companyId,
        string externalEmployeeId,
        string employeeCode,
        string fullName,
        decimal baseSalary)
    {
        if (companyId == CompanyId.Empty)
            throw new BusinessRuleViolationException("CompanyRequired", "Employee must belong to a company.");

        if (string.IsNullOrWhiteSpace(externalEmployeeId))
            throw new BusinessRuleViolationException("ExternalIdRequired", "ExternalEmployeeId is required.");

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new BusinessRuleViolationException("CodeRequired", "EmployeeCode is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("NameRequired", "Employee full name is required.");

        if (baseSalary < 0)
            throw new BusinessRuleViolationException("InvalidBaseSalary", "Base salary cannot be negative.");
    }
}

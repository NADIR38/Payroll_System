using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Employee.Commands.SyncEmployee;

public class SyncEmployeeCommandHandler : IRequestHandler<SyncEmployeeCommand, SyncEmployeeResult>
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public SyncEmployeeCommandHandler(
        IEmployeePayrollProfileRepository employeeRepository,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<SyncEmployeeResult> Handle(SyncEmployeeCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var companyId = new CompanyId(request.CompanyId);
        var branchId = new BranchId(request.BranchId);
        var deptId = new DepartmentId(request.DepartmentId);
        var desigId = new DesignationId(request.DesignationId);
        var costCenterId = request.CostCenterId.HasValue ? new CostCenterId(request.CostCenterId.Value) : (CostCenterId?)null;
        var structureId = new SalaryStructureId(request.SalaryStructureId);

        // OPTIMIZATION: Combine all 7 validation checks (Company, Calendar Guard, Branch, Dept, Desig, CostCenter, Structure)
        // into a single optimized DB round-trip query to eliminate N+1 latency.
        var validationData = await _dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new
            {
                CompanyExists = true,
                HasCalendar = _dbContext.PayrollCalendars.Any(cal => cal.CompanyId == companyId),
                BranchValid = _dbContext.Branches.Any(b => b.Id == branchId && b.CompanyId == companyId),
                DeptValid = _dbContext.Departments.Any(d => d.Id == deptId && d.CompanyId == companyId),
                DesigValid = _dbContext.Designations.Any(ds => ds.Id == desigId && ds.CompanyId == companyId),
                CostCenterValid = costCenterId == null || _dbContext.CostCenters.Any(cc => cc.Id == costCenterId && cc.CompanyId == companyId),
                StructureValid = _dbContext.SalaryStructures.Any(s => s.Id == structureId && s.CompanyId == companyId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (validationData == null)
            throw new NotFoundException($"Company with ID '{request.CompanyId}' was not found.");

        if (!validationData.HasCalendar)
        {
            throw new BusinessRuleViolationException(
                "CALENDAR_REQUIRED",
                "At least one PayrollCalendar must exist for the company before syncing employees.");
        }

        if (!validationData.BranchValid)
            throw new BusinessRuleViolationException("INVALID_BRANCH", "Branch not found or does not belong to this company.");

        if (!validationData.DeptValid)
            throw new BusinessRuleViolationException("INVALID_DEPARTMENT", "Department not found or does not belong to this company.");

        if (!validationData.DesigValid)
            throw new BusinessRuleViolationException("INVALID_DESIGNATION", "Designation not found or does not belong to this company.");

        if (!validationData.CostCenterValid)
            throw new BusinessRuleViolationException("INVALID_COST_CENTER", "Cost center not found or does not belong to this company.");

        if (!validationData.StructureValid)
        {
            throw new BusinessRuleViolationException(
                "INVALID_SALARY_STRUCTURE",
                "An employee cannot be assigned to a SalaryStructure that does not belong to their company.");
        }

        // Upsert employee profile by (ExternalEmployeeId + CompanyId)
        var existingProfile = await _employeeRepository.GetByExternalIdAsync(companyId, request.ExternalEmployeeId, cancellationToken);
        string action;
        EmployeePayrollProfile profile;

        if (existingProfile != null)
        {
            profile = existingProfile;
            action = "Updated";

            profile.Update(
                request.FullName,
                branchId,
                deptId,
                desigId,
                costCenterId,
                structureId,
                request.BaseSalary,
                request.JoiningDate,
                null,
                request.AttendanceDeductionOptIn,
                EmployeeStatus.Active,
                changedBy: request.SyncedBy ?? "ERP_SYNC",
                changeReason: "Re-synced employee data from ERP");

            _employeeRepository.Update(profile);
        }
        else
        {
            action = "Created";

            profile = EmployeePayrollProfile.Create(
                companyId,
                request.ExternalEmployeeId,
                string.IsNullOrWhiteSpace(request.EmployeeCode) ? $"EMP-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}" : request.EmployeeCode,
                request.FullName,
                branchId,
                deptId,
                desigId,
                costCenterId,
                structureId,
                request.BaseSalary,
                request.JoiningDate,
                request.AttendanceDeductionOptIn,
                syncedBy: request.SyncedBy ?? "ERP_SYNC");

            await _employeeRepository.AddAsync(profile, cancellationToken);
        }

        // Process bank account if provided
        if (!string.IsNullOrWhiteSpace(request.IBAN) && !string.IsNullOrWhiteSpace(request.BankName))
        {
            profile.AddBankAccount(
                request.BankName,
                request.AccountTitle!,
                request.AccountNumber!,
                request.IBAN,
                request.BranchCode,
                isPrimary: true);
        }

        try
        {
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Concurrent sync request inserted record between check and commit: fallback to Update
            var retryProfile = await _employeeRepository.GetByExternalIdAsync(companyId, request.ExternalEmployeeId, cancellationToken);
            if (retryProfile != null)
            {
                retryProfile.Update(
                    request.FullName,
                    branchId,
                    deptId,
                    desigId,
                    costCenterId,
                    structureId,
                    request.BaseSalary,
                    request.JoiningDate,
                    null,
                    request.AttendanceDeductionOptIn,
                    EmployeeStatus.Active,
                    changedBy: request.SyncedBy ?? "ERP_SYNC",
                    changeReason: "Re-synced employee data from ERP (concurrency fallback)");

                _employeeRepository.Update(retryProfile);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                profile = retryProfile;
                action = "Updated";
            }
            else
            {
                throw;
            }
        }

        return new SyncEmployeeResult(profile.Id.Value, action);
    }
}

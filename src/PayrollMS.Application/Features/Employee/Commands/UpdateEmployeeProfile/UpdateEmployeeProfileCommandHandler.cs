using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Enums;
using PayrollMS.Domain.Exceptions;
using PayrollMS.Domain.Interfaces.Repositories;

namespace PayrollMS.Application.Features.Employee.Commands.UpdateEmployeeProfile;

public class UpdateEmployeeProfileCommandHandler : IRequestHandler<UpdateEmployeeProfileCommand>
{
    private readonly IEmployeePayrollProfileRepository _employeeRepository;
    private readonly ISalaryStructureRepository _salaryStructureRepository;
    private readonly IAppDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeProfileCommandHandler(
        IEmployeePayrollProfileRepository employeeRepository,
        ISalaryStructureRepository salaryStructureRepository,
        IAppDbContext dbContext,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _salaryStructureRepository = salaryStructureRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateEmployeeProfileCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var profileId = new EmployeePayrollProfileId(request.Id);
        var profile = await _employeeRepository.GetByIdForUpdateAsync(profileId, cancellationToken);
        if (profile == null)
            throw new NotFoundException($"Employee profile with ID '{request.Id}' was not found.");

        var branchId = new BranchId(request.BranchId);
        var deptId = new DepartmentId(request.DepartmentId);
        var desigId = new DesignationId(request.DesignationId);
        var costCenterId = request.CostCenterId.HasValue ? new CostCenterId(request.CostCenterId.Value) : (CostCenterId?)null;
        var structureId = new SalaryStructureId(request.SalaryStructureId);

        // Validate all organizational units and structure belong to the employee's company in 1 round-trip
        var validationData = await _dbContext.Companies
            .AsNoTracking()
            .Where(c => c.Id == profile.CompanyId)
            .Select(c => new
            {
                BranchValid = _dbContext.Branches.Any(b => b.Id == branchId && b.CompanyId == profile.CompanyId),
                DeptValid = _dbContext.Departments.Any(d => d.Id == deptId && d.CompanyId == profile.CompanyId),
                DesigValid = _dbContext.Designations.Any(ds => ds.Id == desigId && ds.CompanyId == profile.CompanyId),
                CostCenterValid = costCenterId == null || _dbContext.CostCenters.Any(cc => cc.Id == costCenterId && cc.CompanyId == profile.CompanyId),
                StructureValid = _dbContext.SalaryStructures.Any(s => s.Id == structureId && s.CompanyId == profile.CompanyId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (validationData == null || !validationData.BranchValid)
            throw new BusinessRuleViolationException("INVALID_BRANCH", "Branch not found or does not belong to the employee's company.");

        if (!validationData.DeptValid)
            throw new BusinessRuleViolationException("INVALID_DEPARTMENT", "Department not found or does not belong to the employee's company.");

        if (!validationData.DesigValid)
            throw new BusinessRuleViolationException("INVALID_DESIGNATION", "Designation not found or does not belong to the employee's company.");

        if (!validationData.CostCenterValid)
            throw new BusinessRuleViolationException("INVALID_COST_CENTER", "Cost center not found or does not belong to the employee's company.");

        if (!validationData.StructureValid)
        {
            throw new BusinessRuleViolationException(
                "INVALID_SALARY_STRUCTURE",
                "Assigned SalaryStructure does not belong to the employee's company.");
        }

        var status = Enum.Parse<EmployeeStatus>(request.Status, ignoreCase: true);

        profile.Update(
            request.FullName,
            new BranchId(request.BranchId),
            new DepartmentId(request.DepartmentId),
            new DesignationId(request.DesignationId),
            request.CostCenterId.HasValue ? new CostCenterId(request.CostCenterId.Value) : null,
            structureId,
            request.BaseSalary,
            request.JoiningDate,
            request.LeavingDate,
            request.AttendanceDeductionOptIn,
            status,
            request.ChangedBy,
            request.ChangeReason);

        _employeeRepository.Update(profile);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using MediatR;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Employee;
using PayrollMS.Domain.Entities.Salary;
using PayrollMS.Domain.Entities.Tenant;

namespace PayrollMS.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IMediator _mediator;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenant currentTenant,
        IMediator mediator) : base(options)
    {
        _currentTenant = currentTenant;
        _mediator = mediator;
    }

    // Module 01 — Tenant
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<PayrollCalendar> PayrollCalendars => Set<PayrollCalendar>();

    // Module 02 — Employee Profile
    public DbSet<EmployeePayrollProfile> EmployeeProfiles => Set<EmployeePayrollProfile>();
    public DbSet<EmployeePayrollProfileHistory> EmployeeProfileHistory => Set<EmployeePayrollProfileHistory>();
    public DbSet<EmployeeBankAccount> EmployeeBankAccounts => Set<EmployeeBankAccount>();

    // Module 03 & 04 — Salary Components & Structures
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryStructureComponent> SalaryStructureComponents => Set<SalaryStructureComponent>();
    public DbSet<AllowanceRule> AllowanceRules => Set<AllowanceRule>();
    public DbSet<DeductionRule> DeductionRules => Set<DeductionRule>();

    // Phase 3 & 4
    public DbSet<PayrollMS.Domain.Entities.Payroll.AttendanceSummary> AttendanceSummaries => Set<PayrollMS.Domain.Entities.Payroll.AttendanceSummary>();
    public DbSet<PayrollMS.Domain.Entities.Payroll.LeaveSummary> LeaveSummaries => Set<PayrollMS.Domain.Entities.Payroll.LeaveSummary>();
    public DbSet<PayrollMS.Domain.Entities.Payroll.PayrollRun> PayrollRuns => Set<PayrollMS.Domain.Entities.Payroll.PayrollRun>();
    public DbSet<PayrollMS.Domain.Entities.Payroll.PayrollEntry> PayrollEntries => Set<PayrollMS.Domain.Entities.Payroll.PayrollEntry>();
    public DbSet<PayrollMS.Domain.Entities.Payroll.PayrollEntryComponent> PayrollEntryComponents => Set<PayrollMS.Domain.Entities.Payroll.PayrollEntryComponent>();
    public DbSet<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowTemplate> ApprovalWorkflowTemplates => Set<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowTemplate>();
    public DbSet<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowStep> ApprovalWorkflowSteps => Set<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowStep>();
    public DbSet<PayrollMS.Domain.Entities.Approval.PayrollApprovalRecord> PayrollApprovalRecords => Set<PayrollMS.Domain.Entities.Approval.PayrollApprovalRecord>();

    // IAppDbContext Interface Explicit Implementations (Returning IQueryable<T>)
    IQueryable<Company> IAppDbContext.Companies => Companies;
    IQueryable<Branch> IAppDbContext.Branches => Branches;
    IQueryable<Department> IAppDbContext.Departments => Departments;
    IQueryable<Designation> IAppDbContext.Designations => Designations;
    IQueryable<CostCenter> IAppDbContext.CostCenters => CostCenters;
    IQueryable<FinancialYear> IAppDbContext.FinancialYears => FinancialYears;
    IQueryable<PayrollCalendar> IAppDbContext.PayrollCalendars => PayrollCalendars;

    IQueryable<EmployeePayrollProfile> IAppDbContext.EmployeeProfiles => EmployeeProfiles;
    IQueryable<EmployeePayrollProfileHistory> IAppDbContext.EmployeeProfileHistory => EmployeeProfileHistory;
    IQueryable<EmployeeBankAccount> IAppDbContext.EmployeeBankAccounts => EmployeeBankAccounts;

    IQueryable<SalaryComponent> IAppDbContext.SalaryComponents => SalaryComponents;
    IQueryable<SalaryStructure> IAppDbContext.SalaryStructures => SalaryStructures;
    IQueryable<SalaryStructureComponent> IAppDbContext.SalaryStructureComponents => SalaryStructureComponents;
    IQueryable<AllowanceRule> IAppDbContext.AllowanceRules => AllowanceRules;
    IQueryable<DeductionRule> IAppDbContext.DeductionRules => DeductionRules;

    IQueryable<PayrollMS.Domain.Entities.Payroll.AttendanceSummary> IAppDbContext.AttendanceSummaries => AttendanceSummaries;
    IQueryable<PayrollMS.Domain.Entities.Payroll.LeaveSummary> IAppDbContext.LeaveSummaries => LeaveSummaries;
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollRun> IAppDbContext.PayrollRuns => PayrollRuns;
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollEntry> IAppDbContext.PayrollEntries => PayrollEntries;
    IQueryable<PayrollMS.Domain.Entities.Payroll.PayrollEntryComponent> IAppDbContext.PayrollEntryComponents => PayrollEntryComponents;
    IQueryable<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowTemplate> IAppDbContext.ApprovalWorkflowTemplates => ApprovalWorkflowTemplates;
    IQueryable<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowStep> IAppDbContext.ApprovalWorkflowSteps => ApprovalWorkflowSteps;
    IQueryable<PayrollMS.Domain.Entities.Approval.PayrollApprovalRecord> IAppDbContext.PayrollApprovalRecords => PayrollApprovalRecords;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Enforce Global Query Filters for tenant isolation and soft deletes
        modelBuilder.Entity<Company>().HasQueryFilter(c => (c.Id == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !c.IsDeleted);
        modelBuilder.Entity<Branch>().HasQueryFilter(b => (b.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !b.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => (d.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !d.IsDeleted);
        modelBuilder.Entity<Designation>().HasQueryFilter(d => (d.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !d.IsDeleted);
        modelBuilder.Entity<CostCenter>().HasQueryFilter(c => (c.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !c.IsDeleted);
        modelBuilder.Entity<FinancialYear>().HasQueryFilter(f => (f.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !f.IsDeleted);
        modelBuilder.Entity<PayrollCalendar>().HasQueryFilter(p => (p.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !p.IsDeleted);

        // Module 02 Query Filters
        modelBuilder.Entity<EmployeePayrollProfile>().HasQueryFilter(e => (e.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !e.IsDeleted);
        modelBuilder.Entity<EmployeePayrollProfileHistory>().HasQueryFilter(h => (h.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !h.IsDeleted);
        modelBuilder.Entity<EmployeeBankAccount>().HasQueryFilter(b => (b.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !b.IsDeleted);

        // Module 03 & 04 Query Filters
        modelBuilder.Entity<SalaryComponent>().HasQueryFilter(s => (s.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !s.IsDeleted);
        modelBuilder.Entity<SalaryStructure>().HasQueryFilter(s => (s.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !s.IsDeleted);
        modelBuilder.Entity<SalaryStructureComponent>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<AllowanceRule>().HasQueryFilter(a => (a.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !a.IsDeleted);
        modelBuilder.Entity<DeductionRule>().HasQueryFilter(d => (d.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !d.IsDeleted);

        // Phase 3 & 4 Query Filters
        modelBuilder.Entity<PayrollMS.Domain.Entities.Payroll.AttendanceSummary>().HasQueryFilter(a => (a.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !a.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Payroll.LeaveSummary>().HasQueryFilter(l => (l.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !l.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Payroll.PayrollRun>().HasQueryFilter(r => (r.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !r.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Payroll.PayrollEntry>().HasQueryFilter(e => (e.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !e.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Payroll.PayrollEntryComponent>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowTemplate>().HasQueryFilter(t => (t.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !t.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Approval.ApprovalWorkflowStep>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<PayrollMS.Domain.Entities.Approval.PayrollApprovalRecord>().HasQueryFilter(r => (r.CompanyId == _currentTenant.CompanyId || _currentTenant.CompanyId == null) && !r.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties();

        var result = await base.SaveChangesAsync(cancellationToken);

        await DispatchDomainEventsAsync();

        return result;
    }

    private void UpdateAuditProperties()
    {
        var userId = _currentTenant.UserId ?? "System";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") is not null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }

                if (entry.Metadata.FindProperty("CreatedBy") is not null)
                {
                    entry.Property("CreatedBy").CurrentValue = userId;
                }

                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty("UpdatedAt") is not null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTimeOffset.UtcNow;
                }

                if (entry.Metadata.FindProperty("UpdatedBy") is not null)
                {
                    entry.Property("UpdatedBy").CurrentValue = userId;
                }
            }
        }
    }

    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker.Entries()
            .Where(x => x.Entity.GetType().GetProperty("DomainEvents") is not null)
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = new List<IDomainEvent>();

        foreach (var entity in domainEntities)
        {
            var eventsProp = entity.GetType().GetProperty("DomainEvents");
            if (eventsProp?.GetValue(entity) is IReadOnlyCollection<IDomainEvent> events)
            {
                domainEvents.AddRange(events);
                entity.GetType().GetMethod("ClearDomainEvents")?.Invoke(entity, null);
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(PayrollMS.Application.Common.Models.DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);
            if (notification is INotification mediatrNotification)
            {
                await _mediator.Publish(mediatrNotification);
            }
        }
    }
}

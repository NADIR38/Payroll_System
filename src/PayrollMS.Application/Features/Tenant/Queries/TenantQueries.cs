using MediatR;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Application.Interfaces;
using PayrollMS.Domain.Exceptions;

namespace PayrollMS.Application.Features.Tenant.Queries;

// ==========================================
// 1. COMPANIES
// ==========================================

public sealed record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyResponse>;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyResponse>
{
    private readonly IAppDbContext _context;

    public GetCompanyByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyResponse> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.CompanyId(request.Id);
        var company = await _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == targetId)
            .Select(c => new CompanyResponse(
                c.Id.Value,
                c.Name,
                c.Code,
                c.LogoUrl,
                c.Address != null ? new AddressDto(c.Address.Street, c.Address.City, c.Address.State, c.Address.Country, c.Address.ZipCode) : null,
                c.ContactEmail != null ? c.ContactEmail.Value : null,
                c.ContactPhone != null ? c.ContactPhone.Value : null,
                c.IsActive,
                c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (company == null)
        {
            throw new NotFoundException($"Company with ID '{request.Id}' was not found.");
        }

        return company;
    }
}

public sealed record ListCompaniesQuery : IRequest<IReadOnlyList<CompanyResponse>>;

public class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, IReadOnlyList<CompanyResponse>>
{
    private readonly IAppDbContext _context;

    public ListCompaniesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyResponse>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Companies
            .AsNoTracking()
            .Select(c => new CompanyResponse(
                c.Id.Value,
                c.Name,
                c.Code,
                c.LogoUrl,
                c.Address != null ? new AddressDto(c.Address.Street, c.Address.City, c.Address.State, c.Address.Country, c.Address.ZipCode) : null,
                c.ContactEmail != null ? c.ContactEmail.Value : null,
                c.ContactPhone != null ? c.ContactPhone.Value : null,
                c.IsActive,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 2. BRANCHES
// ==========================================

public sealed record GetBranchByIdQuery(Guid Id) : IRequest<BranchResponse>;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchResponse>
{
    private readonly IAppDbContext _context;

    public GetBranchByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BranchResponse> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.BranchId(request.Id);
        var branch = await _context.Branches
            .AsNoTracking()
            .Where(b => b.Id == targetId)
            .Select(b => new BranchResponse(
                b.Id.Value,
                b.CompanyId.Value,
                b.Name,
                b.Code,
                b.Address,
                b.IsActive,
                b.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (branch == null)
        {
            throw new NotFoundException($"Branch with ID '{request.Id}' was not found.");
        }

        return branch;
    }
}

public sealed record ListBranchesQuery(Guid CompanyId) : IRequest<IReadOnlyList<BranchResponse>>;

public class ListBranchesQueryHandler : IRequestHandler<ListBranchesQuery, IReadOnlyList<BranchResponse>>
{
    private readonly IAppDbContext _context;

    public ListBranchesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BranchResponse>> Handle(ListBranchesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Branches
            .AsNoTracking()
            .Where(b => b.CompanyId == new Domain.Common.CompanyId(request.CompanyId))
            .Select(b => new BranchResponse(
                b.Id.Value,
                b.CompanyId.Value,
                b.Name,
                b.Code,
                b.Address,
                b.IsActive,
                b.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 3. DEPARTMENTS
// ==========================================

public sealed record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentResponse>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentResponse>
{
    private readonly IAppDbContext _context;

    public GetDepartmentByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentResponse> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.DepartmentId(request.Id);
        var department = await _context.Departments
            .AsNoTracking()
            .Where(d => d.Id == targetId)
            .Select(d => new DepartmentResponse(
                d.Id.Value,
                d.CompanyId.Value,
                d.BranchId != null ? d.BranchId.Value.Value : null,
                d.Name,
                d.Code,
                d.IsActive,
                d.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (department == null)
        {
            throw new NotFoundException($"Department with ID '{request.Id}' was not found.");
        }

        return department;
    }
}

public sealed record ListDepartmentsQuery(Guid CompanyId) : IRequest<IReadOnlyList<DepartmentResponse>>;

public class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, IReadOnlyList<DepartmentResponse>>
{
    private readonly IAppDbContext _context;

    public ListDepartmentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DepartmentResponse>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.CompanyId == new Domain.Common.CompanyId(request.CompanyId))
            .Select(d => new DepartmentResponse(
                d.Id.Value,
                d.CompanyId.Value,
                d.BranchId != null ? d.BranchId.Value.Value : null,
                d.Name,
                d.Code,
                d.IsActive,
                d.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 4. DESIGNATIONS
// ==========================================

public sealed record GetDesignationByIdQuery(Guid Id) : IRequest<DesignationResponse>;

public class GetDesignationByIdQueryHandler : IRequestHandler<GetDesignationByIdQuery, DesignationResponse>
{
    private readonly IAppDbContext _context;

    public GetDesignationByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DesignationResponse> Handle(GetDesignationByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.DesignationId(request.Id);
        var designation = await _context.Designations
            .AsNoTracking()
            .Where(d => d.Id == targetId)
            .Select(d => new DesignationResponse(
                d.Id.Value,
                d.CompanyId.Value,
                d.Name,
                d.Code,
                d.Grade,
                d.IsActive,
                d.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (designation == null)
        {
            throw new NotFoundException($"Designation with ID '{request.Id}' was not found.");
        }

        return designation;
    }
}

public sealed record ListDesignationsQuery(Guid CompanyId) : IRequest<IReadOnlyList<DesignationResponse>>;

public class ListDesignationsQueryHandler : IRequestHandler<ListDesignationsQuery, IReadOnlyList<DesignationResponse>>
{
    private readonly IAppDbContext _context;

    public ListDesignationsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DesignationResponse>> Handle(ListDesignationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Designations
            .AsNoTracking()
            .Where(d => d.CompanyId == new Domain.Common.CompanyId(request.CompanyId))
            .Select(d => new DesignationResponse(
                d.Id.Value,
                d.CompanyId.Value,
                d.Name,
                d.Code,
                d.Grade,
                d.IsActive,
                d.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 5. COST CENTERS
// ==========================================

public sealed record GetCostCenterByIdQuery(Guid Id) : IRequest<CostCenterResponse>;

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterResponse>
{
    private readonly IAppDbContext _context;

    public GetCostCenterByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CostCenterResponse> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.CostCenterId(request.Id);
        var costCenter = await _context.CostCenters
            .AsNoTracking()
            .Where(c => c.Id == targetId)
            .Select(c => new CostCenterResponse(
                c.Id.Value,
                c.CompanyId.Value,
                c.Name,
                c.Code,
                c.IsActive,
                c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (costCenter == null)
        {
            throw new NotFoundException($"Cost Center with ID '{request.Id}' was not found.");
        }

        return costCenter;
    }
}

public sealed record ListCostCentersQuery(Guid CompanyId) : IRequest<IReadOnlyList<CostCenterResponse>>;

public class ListCostCentersQueryHandler : IRequestHandler<ListCostCentersQuery, IReadOnlyList<CostCenterResponse>>
{
    private readonly IAppDbContext _context;

    public ListCostCentersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CostCenterResponse>> Handle(ListCostCentersQuery request, CancellationToken cancellationToken)
    {
        return await _context.CostCenters
            .AsNoTracking()
            .Where(c => c.CompanyId == new Domain.Common.CompanyId(request.CompanyId))
            .Select(c => new CostCenterResponse(
                c.Id.Value,
                c.CompanyId.Value,
                c.Name,
                c.Code,
                c.IsActive,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 6. FINANCIAL YEARS
// ==========================================

public sealed record GetFinancialYearByIdQuery(Guid Id) : IRequest<FinancialYearResponse>;

public class GetFinancialYearByIdQueryHandler : IRequestHandler<GetFinancialYearByIdQuery, FinancialYearResponse>
{
    private readonly IAppDbContext _context;

    public GetFinancialYearByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialYearResponse> Handle(GetFinancialYearByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.FinancialYearId(request.Id);
        var financialYear = await _context.FinancialYears
            .AsNoTracking()
            .Where(f => f.Id == targetId)
            .Select(f => new FinancialYearResponse(
                f.Id.Value,
                f.CompanyId.Value,
                f.Label,
                f.StartDate,
                f.EndDate,
                f.IsCurrent,
                f.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (financialYear == null)
        {
            throw new NotFoundException($"Financial Year with ID '{request.Id}' was not found.");
        }

        return financialYear;
    }
}

public sealed record ListFinancialYearsQuery(Guid CompanyId) : IRequest<IReadOnlyList<FinancialYearResponse>>;

public class ListFinancialYearsQueryHandler : IRequestHandler<ListFinancialYearsQuery, IReadOnlyList<FinancialYearResponse>>
{
    private readonly IAppDbContext _context;

    public ListFinancialYearsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FinancialYearResponse>> Handle(ListFinancialYearsQuery request, CancellationToken cancellationToken)
    {
        return await _context.FinancialYears
            .AsNoTracking()
            .Where(f => f.CompanyId == new Domain.Common.CompanyId(request.CompanyId))
            .Select(f => new FinancialYearResponse(
                f.Id.Value,
                f.CompanyId.Value,
                f.Label,
                f.StartDate,
                f.EndDate,
                f.IsCurrent,
                f.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

// ==========================================
// 7. PAYROLL CALENDARS
// ==========================================

public sealed record GetPayrollCalendarByIdQuery(Guid Id) : IRequest<PayrollCalendarResponse>;

public class GetPayrollCalendarByIdQueryHandler : IRequestHandler<GetPayrollCalendarByIdQuery, PayrollCalendarResponse>
{
    private readonly IAppDbContext _context;

    public GetPayrollCalendarByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollCalendarResponse> Handle(GetPayrollCalendarByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new Domain.Common.PayrollCalendarId(request.Id);
        var calendar = await _context.PayrollCalendars
            .AsNoTracking()
            .Where(p => p.Id == targetId)
            .Select(p => new PayrollCalendarResponse(
                p.Id.Value,
                p.CompanyId.Value,
                p.FinancialYearId.Value,
                p.Month,
                p.Year,
                p.PayrollFreezeDate,
                p.PaymentDate,
                p.WorkingDays,
                p.Holidays,
                p.Status.ToString(),
                p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (calendar == null)
        {
            throw new NotFoundException($"Payroll Calendar with ID '{request.Id}' was not found.");
        }

        return calendar;
    }
}

public sealed record ListPayrollCalendarsQuery(Guid CompanyId, Guid FinancialYearId) : IRequest<IReadOnlyList<PayrollCalendarResponse>>;

public class ListPayrollCalendarsQueryHandler : IRequestHandler<ListPayrollCalendarsQuery, IReadOnlyList<PayrollCalendarResponse>>
{
    private readonly IAppDbContext _context;

    public ListPayrollCalendarsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayrollCalendarResponse>> Handle(ListPayrollCalendarsQuery request, CancellationToken cancellationToken)
    {
        return await _context.PayrollCalendars
            .AsNoTracking()
            .Where(p => p.CompanyId == new Domain.Common.CompanyId(request.CompanyId) && (request.FinancialYearId == Guid.Empty || p.FinancialYearId == new Domain.Common.FinancialYearId(request.FinancialYearId)))
            .Select(p => new PayrollCalendarResponse(
                p.Id.Value,
                p.CompanyId.Value,
                p.FinancialYearId.Value,
                p.Month,
                p.Year,
                p.PayrollFreezeDate,
                p.PaymentDate,
                p.WorkingDays,
                p.Holidays,
                p.Status.ToString(),
                p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

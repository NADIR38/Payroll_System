namespace PayrollMS.Application.Features.Tenant;

/// <summary>
/// Address details for a tenant company.
/// </summary>
public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

/// <summary>
/// Tenant Company response DTO.
/// </summary>
public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string Code,
    string? LogoUrl,
    AddressDto? Address,
    string? ContactEmail,
    string? ContactPhone,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Company Branch response DTO.
/// </summary>
public sealed record BranchResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Code,
    string? Address,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Company Department response DTO.
/// </summary>
public sealed record DepartmentResponse(
    Guid Id,
    Guid CompanyId,
    Guid? BranchId,
    string Name,
    string Code,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Designation response DTO.
/// </summary>
public sealed record DesignationResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Code,
    string? Grade,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Cost Center response DTO.
/// </summary>
public sealed record CostCenterResponse(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Code,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>
/// Financial Year response DTO.
/// </summary>
public sealed record FinancialYearResponse(
    Guid Id,
    Guid CompanyId,
    string Label,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    DateTimeOffset CreatedAt);

/// <summary>
/// Payroll Calendar response DTO.
/// </summary>
public sealed record PayrollCalendarResponse(
    Guid Id,
    Guid CompanyId,
    Guid FinancialYearId,
    int Month,
    int Year,
    DateOnly PayrollFreezeDate,
    DateOnly PaymentDate,
    int WorkingDays,
    string? Holidays,
    string Status,
    DateTimeOffset CreatedAt);

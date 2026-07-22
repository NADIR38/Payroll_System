using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using PayrollMS.Domain.Interfaces.Repositories;
using PayrollMS.Domain.Interfaces.Services;
using PayrollMS.Application.Interfaces;
using PayrollMS.Application.Common.Extensions;
using PayrollMS.Infrastructure.Persistence;
using PayrollMS.Infrastructure.Persistence.Repositories;
using PayrollMS.Infrastructure.Services;

namespace PayrollMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=payrollms_db;Username=postgres;Password=postgres";

        // Configure Application layer queryable extensions delegates to use EF Core
        ConfigureQueryableExtensions();

        // Register AppDbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Register Formula Evaluator Engine
        services.AddSingleton<IFormulaEvaluator, FormulaEvaluatorService>();

        // Register repositories
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();
        services.AddScoped<IPayrollCalendarRepository, PayrollCalendarRepository>();

        // Module 02, 03, 04 Repositories
        services.AddScoped<IEmployeePayrollProfileRepository, EmployeePayrollProfileRepository>();
        services.AddScoped<ISalaryComponentRepository, SalaryComponentRepository>();
        services.AddScoped<ISalaryStructureRepository, SalaryStructureRepository>();

        // Phase 3 & 4 Repositories & Services
        services.AddScoped<IAttendanceSummaryRepository, AttendanceSummaryRepository>();
        services.AddScoped<ILeaveSummaryRepository, LeaveSummaryRepository>();
        services.AddScoped<IPayrollRunRepository, PayrollRunRepository>();
        services.AddScoped<IPayrollEntryRepository, PayrollEntryRepository>();
        services.AddScoped<IApprovalWorkflowTemplateRepository, ApprovalWorkflowTemplateRepository>();
        services.AddScoped<IPayrollApprovalRecordRepository, PayrollApprovalRecordRepository>();

        services.AddScoped<IPayrollCalculator, PayrollCalculatorService>();
        services.AddScoped<Workers.PayrollGenerationWorker>();

        // Register Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Math.Min(Environment.ProcessorCount, 4);
        });

        return services;
    }

    private static void ConfigureQueryableExtensions()
    {
        QueryableExtensions.Configure(
            toListAsync: async (query, ct) =>
            {
                var elemType = query.GetType().GetGenericArguments()[0];
                var method = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethod(nameof(EntityFrameworkQueryableExtensions.ToListAsync))!
                    .MakeGenericMethod(elemType);
                var task = (Task)method.Invoke(null, new object[] { query, ct })!;
                await task;
                return (System.Collections.IList)task.GetType().GetProperty("Result")!.GetValue(task)!;
            },
            firstOrDefaultAsync: async (query, predicate, ct) =>
            {
                var elemType = query.GetType().GetGenericArguments()[0];
                MethodInfo method;
                object[] args;

                if (predicate != null)
                {
                    method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                        .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync) && m.GetParameters().Length == 3)
                        .MakeGenericMethod(elemType);
                    args = new object[] { query, predicate, ct };
                }
                else
                {
                    method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                        .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync) && m.GetParameters().Length == 2)
                        .MakeGenericMethod(elemType);
                    args = new object[] { query, ct };
                }

                var task = (Task)method.Invoke(null, args)!;
                await task;
                return task.GetType().GetProperty("Result")!.GetValue(task);
            },
            anyAsync: async (query, predicate, ct) =>
            {
                var elemType = query.GetType().GetGenericArguments()[0];
                MethodInfo method;
                object[] args;

                if (predicate != null)
                {
                    method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                        .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) && m.GetParameters().Length == 3)
                        .MakeGenericMethod(elemType);
                    args = new object[] { query, predicate, ct };
                }
                else
                {
                    method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                        .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) && m.GetParameters().Length == 2)
                        .MakeGenericMethod(elemType);
                    args = new object[] { query, ct };
                }

                var task = (Task<bool>)method.Invoke(null, args)!;
                return await task;
            },
            asNoTracking: query =>
            {
                var elemType = query.GetType().GetGenericArguments()[0];
                var method = typeof(EntityFrameworkQueryableExtensions)
                    .GetMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!
                    .MakeGenericMethod(elemType);
                return method.Invoke(null, new object[] { query })!;
            },
            include: (query, path) =>
            {
                var elemType = query.GetType().GetGenericArguments()[0];
                var propType = path.GetType().GetGenericArguments()[0].GetGenericArguments()[1];
                var method = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                    .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.Include) && m.GetParameters().Length == 2)
                    .MakeGenericMethod(elemType, propType);
                return method.Invoke(null, new object[] { query, path })!;
            }
        );
    }
}

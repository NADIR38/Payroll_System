using PayrollMS.Application;
using PayrollMS.Infrastructure;
using PayrollMS.Application.Interfaces;
using PayrollMS.Api.Services;
using PayrollMS.Api.Infrastructure;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add API services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

// Register global exception handling & problem details
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Must be first to capture all downstream errors

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthorization();

// Mount Hangfire Dashboard
app.UseHangfireDashboard();

app.MapControllers();

app.Run();

public partial class Program { }

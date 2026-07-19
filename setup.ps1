# Initialize Git
git init
git remote add origin https://github.com/NADIR38/Payroll_System.git
git checkout -b feature/initial-setup

# Create .NET Solution and Projects
dotnet new sln -n PayrollMS
dotnet new classlib -n PayrollMS.Domain -o src/PayrollMS.Domain
dotnet new classlib -n PayrollMS.Application -o src/PayrollMS.Application
dotnet new classlib -n PayrollMS.Infrastructure -o src/PayrollMS.Infrastructure
dotnet new webapi -n PayrollMS.Api -o src/PayrollMS.Api

# Add projects to solution
dotnet sln PayrollMS.sln add src/PayrollMS.Domain/PayrollMS.Domain.csproj src/PayrollMS.Application/PayrollMS.Application.csproj src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj src/PayrollMS.Api/PayrollMS.Api.csproj

# Add References
dotnet add src/PayrollMS.Application/PayrollMS.Application.csproj reference src/PayrollMS.Domain/PayrollMS.Domain.csproj
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj reference src/PayrollMS.Application/PayrollMS.Application.csproj
dotnet add src/PayrollMS.Api/PayrollMS.Api.csproj reference src/PayrollMS.Application/PayrollMS.Application.csproj src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj

# Add NuGet Packages
dotnet add src/PayrollMS.Application/PayrollMS.Application.csproj package MediatR
dotnet add src/PayrollMS.Application/PayrollMS.Application.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package NCalc2
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package QuestPDF
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package Hangfire.PostgreSql
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package MailKit
dotnet add src/PayrollMS.Infrastructure/PayrollMS.Infrastructure.csproj package ClosedXML
dotnet add src/PayrollMS.Api/PayrollMS.Api.csproj package Serilog.AspNetCore
dotnet add src/PayrollMS.Api/PayrollMS.Api.csproj package Microsoft.EntityFrameworkCore.Design

# Add .gitignore
dotnet new gitignore

# Scaffold Next.js
npx -y create-next-app@latest payrollms-web --ts --tailwind --eslint --app --src-dir --import-alias "@/*" --use-npm

# Output success
Write-Host "Setup script completed."

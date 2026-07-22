using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayrollMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAndSalaryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payroll_employee_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEmployeeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DesignationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AttendanceDeductionOptIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    JoiningDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LeavingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_employee_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_salary_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CalculationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_salary_components", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_salary_structures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_salary_structures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_employee_bank_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeePayrollProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AccountTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IBAN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BranchCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_employee_bank_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_employee_bank_accounts_payroll_employee_profiles_Em~",
                        column: x => x.EmployeePayrollProfileId,
                        principalTable: "payroll_employee_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_employee_profile_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeePayrollProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEmployeeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_employee_profile_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_employee_profile_history_payroll_employee_profiles_~",
                        column: x => x.EmployeePayrollProfileId,
                        principalTable: "payroll_employee_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_salary_structure_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormulaExpression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FixedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_salary_structure_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_salary_structure_components_payroll_salary_componen~",
                        column: x => x.SalaryComponentId,
                        principalTable: "payroll_salary_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_salary_structure_components_payroll_salary_structur~",
                        column: x => x.SalaryStructureId,
                        principalTable: "payroll_salary_structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_allowance_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryStructureComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConditionExpression = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_allowance_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_allowance_rules_payroll_salary_structure_components~",
                        column: x => x.SalaryStructureComponentId,
                        principalTable: "payroll_salary_structure_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_deduction_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryStructureComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsOptIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GracePeriodMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DeductionFormula = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_deduction_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_deduction_rules_payroll_salary_structure_components~",
                        column: x => x.SalaryStructureComponentId,
                        principalTable: "payroll_salary_structure_components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_allowance_rules_SalaryStructureComponentId",
                table: "payroll_allowance_rules",
                column: "SalaryStructureComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_deduction_rules_SalaryStructureComponentId",
                table: "payroll_deduction_rules",
                column: "SalaryStructureComponentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_employee_bank_accounts_CompanyId_EmployeePayrollPro~",
                table: "payroll_employee_bank_accounts",
                columns: new[] { "CompanyId", "EmployeePayrollProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_employee_bank_accounts_EmployeePayrollProfileId",
                table: "payroll_employee_bank_accounts",
                column: "EmployeePayrollProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_employee_profile_history_CompanyId_EmployeePayrollP~",
                table: "payroll_employee_profile_history",
                columns: new[] { "CompanyId", "EmployeePayrollProfileId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_employee_profile_history_EmployeePayrollProfileId",
                table: "payroll_employee_profile_history",
                column: "EmployeePayrollProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_employee_profiles_CompanyId_ExternalEmployeeId",
                table: "payroll_employee_profiles",
                columns: new[] { "CompanyId", "ExternalEmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_components_CompanyId_Code",
                table: "payroll_salary_components",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_structure_components_SalaryComponentId",
                table: "payroll_salary_structure_components",
                column: "SalaryComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_structure_components_SalaryStructureId_Seque~",
                table: "payroll_salary_structure_components",
                columns: new[] { "SalaryStructureId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_structures_CompanyId_Code",
                table: "payroll_salary_structures",
                columns: new[] { "CompanyId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payroll_allowance_rules");

            migrationBuilder.DropTable(
                name: "payroll_deduction_rules");

            migrationBuilder.DropTable(
                name: "payroll_employee_bank_accounts");

            migrationBuilder.DropTable(
                name: "payroll_employee_profile_history");

            migrationBuilder.DropTable(
                name: "payroll_salary_structure_components");

            migrationBuilder.DropTable(
                name: "payroll_employee_profiles");

            migrationBuilder.DropTable(
                name: "payroll_salary_components");

            migrationBuilder.DropTable(
                name: "payroll_salary_structures");
        }
    }
}

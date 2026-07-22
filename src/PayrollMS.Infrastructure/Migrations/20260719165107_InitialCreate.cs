using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayrollMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payroll_companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddressStreet = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AddressCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_branches_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_cost_centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_cost_centers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_cost_centers_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_designations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Grade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_designations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_designations_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_financial_years",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_financial_years", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_financial_years_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_departments_payroll_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "payroll_branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_payroll_departments_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payroll_calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    PayrollFreezeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkingDays = table.Column<int>(type: "integer", nullable: false),
                    Holidays = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_calendars_payroll_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "payroll_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_calendars_payroll_financial_years_FinancialYearId",
                        column: x => x.FinancialYearId,
                        principalTable: "payroll_financial_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_branches_CompanyId_Code",
                table: "payroll_branches",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calendars_CompanyId_FinancialYearId_Month_Year",
                table: "payroll_calendars",
                columns: new[] { "CompanyId", "FinancialYearId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_calendars_FinancialYearId",
                table: "payroll_calendars",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_companies_Code",
                table: "payroll_companies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_cost_centers_CompanyId_Code",
                table: "payroll_cost_centers",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_departments_BranchId",
                table: "payroll_departments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_departments_CompanyId_Code",
                table: "payroll_departments",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_designations_CompanyId_Code",
                table: "payroll_designations",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_financial_years_CompanyId_Label",
                table: "payroll_financial_years",
                columns: new[] { "CompanyId", "Label" },
                unique: true);

            // Enable RLS and create tenant isolation policies for tables with CompanyId
            var tenantTables = new[] {
                "payroll_branches",
                "payroll_cost_centers",
                "payroll_designations",
                "payroll_financial_years",
                "payroll_departments",
                "payroll_calendars"
            };

            foreach (var table in tenantTables)
            {
                migrationBuilder.Sql($"ALTER TABLE {table} ENABLE ROW LEVEL SECURITY;");
                migrationBuilder.Sql($"CREATE POLICY tenant_isolation_policy ON {table} FOR ALL USING (\"CompanyId\" = NULLIF(current_setting('app.current_company_id', true), '')::uuid);");
            }

            // Enable RLS for payroll_companies checking by ID itself
            migrationBuilder.Sql("ALTER TABLE payroll_companies ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("CREATE POLICY tenant_isolation_policy ON payroll_companies FOR ALL USING (\"Id\" = NULLIF(current_setting('app.current_company_id', true), '')::uuid);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop policies before dropping tables
            var tenantTables = new[] {
                "payroll_branches",
                "payroll_cost_centers",
                "payroll_designations",
                "payroll_financial_years",
                "payroll_departments",
                "payroll_calendars",
                "payroll_companies"
            };

            foreach (var table in tenantTables)
            {
                migrationBuilder.Sql($"DROP POLICY IF EXISTS tenant_isolation_policy ON {table};");
            }

            migrationBuilder.DropTable(
                name: "payroll_calendars");

            migrationBuilder.DropTable(
                name: "payroll_cost_centers");

            migrationBuilder.DropTable(
                name: "payroll_departments");

            migrationBuilder.DropTable(
                name: "payroll_designations");

            migrationBuilder.DropTable(
                name: "payroll_financial_years");

            migrationBuilder.DropTable(
                name: "payroll_branches");

            migrationBuilder.DropTable(
                name: "payroll_companies");
        }
    }
}

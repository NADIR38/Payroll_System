using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayrollMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollAndAttendanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payroll_salary_structure_components_SalaryStructureId_Seque~",
                table: "payroll_salary_structure_components");

            migrationBuilder.CreateTable(
                name: "payroll_approval_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_approval_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_approval_workflow_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_payroll_approval_workflow_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_attendance_summaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEmployeeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    WorkingDays = table.Column<int>(type: "integer", nullable: false),
                    AbsentDays = table.Column<int>(type: "integer", nullable: false),
                    LateDays = table.Column<int>(type: "integer", nullable: false),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    HalfDays = table.Column<int>(type: "integer", nullable: false),
                    Holidays = table.Column<int>(type: "integer", nullable: false),
                    Weekends = table.Column<int>(type: "integer", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_attendance_summaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_leave_summaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEmployeeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MedicalLeaveDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CasualLeaveDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    HalfDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_leave_summaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RunType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FilterBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    FilterDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalEmployees = table.Column<int>(type: "integer", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GeneratedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RunVersion = table.Column<int>(type: "integer", nullable: false),
                    ParentRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payroll_approval_workflow_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RequiredRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    SLAHours = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_approval_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_approval_workflow_steps_payroll_approval_workflow_t~",
                        column: x => x.WorkflowTemplateId,
                        principalTable: "payroll_approval_workflow_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEmployeeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DepartmentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DesignationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IBAN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    WorkingDays = table.Column<int>(type: "integer", nullable: false),
                    AbsentDays = table.Column<int>(type: "integer", nullable: false),
                    LateDays = table.Column<int>(type: "integer", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_entries_payroll_runs_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "payroll_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_entry_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComponentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FormulaUsed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    IsManualOverride = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_entry_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_entry_components_payroll_entries_PayrollEntryId",
                        column: x => x.PayrollEntryId,
                        principalTable: "payroll_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_structure_components_SalaryStructureId_Seque~",
                table: "payroll_salary_structure_components",
                columns: new[] { "SalaryStructureId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_approval_records_PayrollRunId_StepOrder",
                table: "payroll_approval_records",
                columns: new[] { "PayrollRunId", "StepOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_approval_workflow_steps_WorkflowTemplateId_StepOrder",
                table: "payroll_approval_workflow_steps",
                columns: new[] { "WorkflowTemplateId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_approval_workflow_templates_CompanyId_Name",
                table: "payroll_approval_workflow_templates",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_attendance_summaries_CompanyId_ExternalEmployeeId_P~",
                table: "payroll_attendance_summaries",
                columns: new[] { "CompanyId", "ExternalEmployeeId", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_attendance_summaries_IdempotencyKey",
                table: "payroll_attendance_summaries",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_entries_CompanyId_PayrollRunId_ExternalEmployeeId",
                table: "payroll_entries",
                columns: new[] { "CompanyId", "PayrollRunId", "ExternalEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_entries_PayrollRunId",
                table: "payroll_entries",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_entry_components_PayrollEntryId",
                table: "payroll_entry_components",
                column: "PayrollEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_leave_summaries_CompanyId_ExternalEmployeeId_Period~",
                table: "payroll_leave_summaries",
                columns: new[] { "CompanyId", "ExternalEmployeeId", "PeriodYear", "PeriodMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_leave_summaries_IdempotencyKey",
                table: "payroll_leave_summaries",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payroll_runs_CompanyId_PeriodYear_PeriodMonth_RunType",
                table: "payroll_runs",
                columns: new[] { "CompanyId", "PeriodYear", "PeriodMonth", "RunType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payroll_approval_records");

            migrationBuilder.DropTable(
                name: "payroll_approval_workflow_steps");

            migrationBuilder.DropTable(
                name: "payroll_attendance_summaries");

            migrationBuilder.DropTable(
                name: "payroll_entry_components");

            migrationBuilder.DropTable(
                name: "payroll_leave_summaries");

            migrationBuilder.DropTable(
                name: "payroll_approval_workflow_templates");

            migrationBuilder.DropTable(
                name: "payroll_entries");

            migrationBuilder.DropTable(
                name: "payroll_runs");

            migrationBuilder.DropIndex(
                name: "IX_payroll_salary_structure_components_SalaryStructureId_Seque~",
                table: "payroll_salary_structure_components");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_salary_structure_components_SalaryStructureId_Seque~",
                table: "payroll_salary_structure_components",
                columns: new[] { "SalaryStructureId", "Sequence" });
        }
    }
}

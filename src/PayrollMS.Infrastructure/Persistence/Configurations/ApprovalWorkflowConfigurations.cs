using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayrollMS.Domain.Common;
using PayrollMS.Domain.Entities.Approval;

namespace PayrollMS.Infrastructure.Persistence.Configurations;

public class ApprovalWorkflowTemplateConfiguration : IEntityTypeConfiguration<ApprovalWorkflowTemplate>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflowTemplate> builder)
    {
        builder.ToTable("payroll_approval_workflow_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new ApprovalWorkflowTemplateId(value));

        builder.Property(t => t.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);

        builder.HasIndex(t => new { t.CompanyId, t.Name }).IsUnique();

        builder.HasMany(t => t.Steps)
            .WithOne()
            .HasForeignKey(s => s.WorkflowTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Version)
            .IsRowVersion();
    }
}

public class ApprovalWorkflowStepConfiguration : IEntityTypeConfiguration<ApprovalWorkflowStep>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflowStep> builder)
    {
        builder.ToTable("payroll_approval_workflow_steps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new ApprovalWorkflowStepId(value));

        builder.Property(s => s.WorkflowTemplateId)
            .HasConversion(id => id.Value, value => new ApprovalWorkflowTemplateId(value))
            .IsRequired();

        builder.Property(s => s.StepName).IsRequired().HasMaxLength(150);
        builder.Property(s => s.RequiredRole).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => new { s.WorkflowTemplateId, s.StepOrder }).IsUnique();

        builder.Property(s => s.Version)
            .IsRowVersion();
    }
}

public class PayrollApprovalRecordConfiguration : IEntityTypeConfiguration<PayrollApprovalRecord>
{
    public void Configure(EntityTypeBuilder<PayrollApprovalRecord> builder)
    {
        builder.ToTable("payroll_approval_records");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new PayrollApprovalRecordId(value));

        builder.Property(r => r.PayrollRunId)
            .HasConversion(id => id.Value, value => new PayrollRunId(value))
            .IsRequired();

        builder.Property(r => r.CompanyId)
            .HasConversion(id => id.Value, value => new CompanyId(value))
            .IsRequired();

        builder.Property(r => r.WorkflowStepId)
            .HasConversion(id => id.Value, value => new ApprovalWorkflowStepId(value))
            .IsRequired();

        builder.Property(r => r.StepName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Action).HasConversion<string>().IsRequired().HasMaxLength(50);
        builder.Property(r => r.ActorUserId).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ActorName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.ActorRole).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Comments).HasMaxLength(500);

        builder.HasIndex(r => new { r.PayrollRunId, r.StepOrder });

        builder.Property(r => r.Version)
            .IsRowVersion();
    }
}

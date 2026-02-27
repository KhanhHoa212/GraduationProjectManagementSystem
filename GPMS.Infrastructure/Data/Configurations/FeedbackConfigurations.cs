using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");
        builder.HasKey(f => f.FeedbackID);
        builder.Property(f => f.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(f => f.EvaluationID).IsUnique();

        builder.HasOne(f => f.Evaluation)
            .WithOne(e => e.Feedback)
            .HasForeignKey<Feedback>(f => f.EvaluationID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FeedbackApprovalConfiguration : IEntityTypeConfiguration<FeedbackApproval>
{
    public void Configure(EntityTypeBuilder<FeedbackApproval> builder)
    {
        builder.ToTable("FeedbackApprovals");
        builder.HasKey(fa => fa.FeedbackID);
        builder.Property(fa => fa.SupervisorComment).HasColumnType("nvarchar(max)");
        builder.Property(fa => fa.IsVisibleToStudent).HasDefaultValue(false);

        builder.Property(fa => fa.ApprovalStatus)
            .HasConversion<string>()
            .HasDefaultValue(ApprovalStatus.Pending)
            .HasMaxLength(20);

        builder.HasOne(fa => fa.Feedback)
            .WithOne(f => f.FeedbackApproval)
            .HasForeignKey<FeedbackApproval>(fa => fa.FeedbackID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fa => fa.Supervisor)
            .WithMany(u => u.FeedbackApprovals)
            .HasForeignKey(fa => fa.SupervisorID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

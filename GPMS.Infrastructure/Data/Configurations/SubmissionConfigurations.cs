using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class SubmissionRequirementConfiguration : IEntityTypeConfiguration<SubmissionRequirement>
{
    public void Configure(EntityTypeBuilder<SubmissionRequirement> builder)
    {
        builder.ToTable("SubmissionRequirements");
        builder.HasKey(sr => sr.RequirementID);
        builder.Property(sr => sr.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(sr => sr.Description).HasMaxLength(500);
        builder.Property(sr => sr.AllowedFormats).HasMaxLength(100).IsUnicode(false);
        builder.Property(sr => sr.MaxFileSizeMB).HasDefaultValue(50);
        builder.Property(sr => sr.IsRequired).HasDefaultValue(true);

        builder.HasOne(sr => sr.ReviewRound)
            .WithMany(rr => rr.SubmissionRequirements)
            .HasForeignKey(sr => sr.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


// Actual implementation moved to SubmissionMapping below

// Fixing typo in previous thought, I'll combine VI and skip the middle-man
public class SubmissionMapping : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");
        builder.HasKey(s => s.SubmissionID);
        builder.Property(s => s.FileUrl).IsRequired().HasMaxLength(500).IsUnicode(false);
        builder.Property(s => s.FileName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.FileSizeMB).HasPrecision(6, 2);
        builder.Property(s => s.SubmittedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(s => s.Version).HasDefaultValue(1);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(s => new { s.RequirementID, s.GroupID, s.Version }).IsUnique();

        builder.HasOne(s => s.Requirement)
            .WithMany(sr => sr.Submissions)
            .HasForeignKey(s => s.RequirementID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Group)
            .WithMany(g => g.Submissions)
            .HasForeignKey(s => s.GroupID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Submitter)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.SubmittedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

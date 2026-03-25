using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class ReviewChecklistConfiguration : IEntityTypeConfiguration<ReviewChecklist>
{
    public void Configure(EntityTypeBuilder<ReviewChecklist> builder)
    {
        builder.ToTable("ReviewChecklists");
        builder.HasKey(rc => rc.ChecklistID);
        builder.Property(rc => rc.Title).IsRequired().HasMaxLength(200);
        builder.Property(rc => rc.Description).HasMaxLength(500);
        builder.Property(rc => rc.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.Property(rc => rc.Type)
            .HasConversion<string>()
            .HasDefaultValue(ChecklistType.YesNo)
            .HasMaxLength(20);

        builder.HasIndex(rc => rc.ReviewRoundID).IsUnique();

        builder.HasOne(rc => rc.ReviewRound)
            .WithOne(rr => rr.ReviewChecklist)
            .HasForeignKey<ReviewChecklist>(rc => rc.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rc => rc.Creator)
            .WithMany(u => u.CreatedChecklists)
            .HasForeignKey(rc => rc.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("ChecklistItems");
        builder.HasKey(ci => ci.ItemID);
        builder.Property(ci => ci.ItemCode).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.Property(ci => ci.ItemContent).IsRequired().HasMaxLength(500);
        builder.Property(ci => ci.ItemName).HasMaxLength(200);
        builder.Property(ci => ci.ItemType).IsRequired().HasMaxLength(10).HasDefaultValue("YesNo");
        builder.Property(ci => ci.Section).HasMaxLength(100);
        builder.Property(ci => ci.OrderIndex).HasDefaultValue(1);

        builder.HasOne(ci => ci.Checklist)
            .WithMany(rc => rc.ChecklistItems)
            .HasForeignKey(ci => ci.ChecklistID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("Evaluations");
        builder.HasKey(e => e.EvaluationID);
        builder.Property(e => e.OverallComment).HasColumnType("nvarchar(max)");
        
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasDefaultValue(EvaluationStatus.Draft)
            .HasMaxLength(20);

        builder.HasIndex(e => new { e.ReviewRoundID, e.ReviewerID, e.GroupID }).IsUnique();

        builder.HasOne(e => e.ReviewRound)
            .WithMany(rr => rr.Evaluations)
            .HasForeignKey(e => e.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Reviewer)
            .WithMany(u => u.Evaluations)
            .HasForeignKey(e => e.ReviewerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Group)
            .WithMany(g => g.Evaluations)
            .HasForeignKey(e => e.GroupID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EvaluationDetailConfiguration : IEntityTypeConfiguration<EvaluationDetail>
{
    public void Configure(EntityTypeBuilder<EvaluationDetail> builder)
    {
        builder.ToTable("EvaluationDetails");
        builder.HasKey(ed => new { ed.EvaluationID, ed.ItemID });
        builder.Property(ed => ed.Assessment).HasMaxLength(20);
        builder.Property(ed => ed.Comment).HasColumnType("nvarchar(max)");

        builder.HasOne(ed => ed.Evaluation)
            .WithMany(e => e.EvaluationDetails)
            .HasForeignKey(ed => ed.EvaluationID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ed => ed.Item)
            .WithMany(ci => ci.EvaluationDetails)
            .HasForeignKey(ed => ed.ItemID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RubricDescriptionConfiguration : IEntityTypeConfiguration<RubricDescription>
{
    public void Configure(EntityTypeBuilder<RubricDescription> builder)
    {
        builder.ToTable("RubricDescriptions");
        builder.HasKey(rd => rd.RubricID);
        builder.Property(rd => rd.GradeLevel).IsRequired().HasMaxLength(20);
        builder.Property(rd => rd.Description).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasOne(rd => rd.Item)
            .WithMany(ci => ci.RubricDescriptions)
            .HasForeignKey(rd => rd.ItemID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

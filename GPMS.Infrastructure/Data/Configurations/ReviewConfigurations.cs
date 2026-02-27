using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class ReviewRoundConfiguration : IEntityTypeConfiguration<ReviewRound>
{
    public void Configure(EntityTypeBuilder<ReviewRound> builder)
    {
        builder.ToTable("ReviewRounds");
        builder.HasKey(rr => rr.ReviewRoundID);
        builder.Property(rr => rr.Description).HasMaxLength(500);

        builder.Property(rr => rr.RoundType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(rr => rr.Status)
            .HasConversion<string>()
            .HasDefaultValue(RoundStatus.Planned)
            .HasMaxLength(20);

        builder.HasIndex(rr => new { rr.SemesterID, rr.RoundNumber }).IsUnique();

        builder.HasOne(rr => rr.Semester)
            .WithMany(s => s.ReviewRounds)
            .HasForeignKey(rr => rr.SemesterID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewSessionInfoConfiguration : IEntityTypeConfiguration<ReviewSessionInfo>
{
    public void Configure(EntityTypeBuilder<ReviewSessionInfo> builder)
    {
        builder.ToTable("ReviewSessionInfo");
        builder.HasKey(rs => rs.SessionID);
        builder.Property(rs => rs.MeetLink).HasMaxLength(500).IsUnicode(false);
        builder.Property(rs => rs.Notes).HasMaxLength(500);

        builder.HasIndex(rs => new { rs.ReviewRoundID, rs.GroupID }).IsUnique();

        builder.HasOne(rs => rs.ReviewRound)
            .WithMany(rr => rr.ReviewSessions)
            .HasForeignKey(rs => rs.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Group)
            .WithMany(g => g.ReviewSessions)
            .HasForeignKey(rs => rs.GroupID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.Room)
            .WithMany(r => r.ReviewSessions)
            .HasForeignKey(rs => rs.RoomID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReviewerAssignmentConfiguration : IEntityTypeConfiguration<ReviewerAssignment>
{
    public void Configure(EntityTypeBuilder<ReviewerAssignment> builder)
    {
        builder.ToTable("ReviewerAssignments");
        builder.HasKey(ra => ra.AssignmentID);
        builder.Property(ra => ra.AssignedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(ra => ra.IsRandom).HasDefaultValue(true);

        builder.HasIndex(ra => new { ra.ReviewRoundID, ra.GroupID, ra.ReviewerID }).IsUnique();

        builder.HasOne(ra => ra.ReviewRound)
            .WithMany(rr => rr.ReviewerAssignments)
            .HasForeignKey(ra => ra.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.Group)
            .WithMany(g => g.ReviewerAssignments)
            .HasForeignKey(ra => ra.GroupID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.Reviewer)
            .WithMany(u => u.ReviewerAssignments)
            .HasForeignKey(ra => ra.ReviewerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ra => ra.AdminWhoAssigned)
            .WithMany(u => u.ReviewersAssignedByMe)
            .HasForeignKey(ra => ra.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

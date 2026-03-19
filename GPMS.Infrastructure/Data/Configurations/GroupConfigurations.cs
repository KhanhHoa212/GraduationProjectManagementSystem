using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class ProjectSupervisorConfiguration : IEntityTypeConfiguration<ProjectSupervisor>
{
    public void Configure(EntityTypeBuilder<ProjectSupervisor> builder)
    {
        builder.ToTable("ProjectSupervisors");
        builder.HasKey(ps => new { ps.ProjectID, ps.LecturerID });

        builder.Property(ps => ps.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ps => ps.AssignedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(ps => ps.Project)
            .WithMany(p => p.ProjectSupervisors)
            .HasForeignKey(ps => ps.ProjectID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Lecturer)
            .WithMany(u => u.SupervisedProjects)
            .HasForeignKey(ps => ps.LecturerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.AdminWhoAssigned)
            .WithMany(u => u.ProjectsAssignedByMe)
            .HasForeignKey(ps => ps.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProjectGroupConfiguration : IEntityTypeConfiguration<ProjectGroup>
{
    public void Configure(EntityTypeBuilder<ProjectGroup> builder)
    {
        builder.ToTable("ProjectGroups");
        builder.HasKey(pg => pg.GroupID);
        builder.Property(pg => pg.GroupName).IsRequired().HasMaxLength(100);
        builder.Property(pg => pg.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(pg => pg.Project)
            .WithMany(p => p.ProjectGroups)
            .HasForeignKey(pg => pg.ProjectID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers");
        builder.HasKey(gm => new { gm.GroupID, gm.UserID });

        builder.Property(gm => gm.RoleInGroup)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(gm => gm.JoinedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.GroupID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gm => gm.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(gm => gm.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GroupRoundProgressConfiguration : IEntityTypeConfiguration<GroupRoundProgress>
{
    public void Configure(EntityTypeBuilder<GroupRoundProgress> builder)
    {
        builder.ToTable("GroupRoundProgresses");
        builder.HasKey(grp => new { grp.GroupID, grp.ReviewRoundID });

        builder.Property(grp => grp.MentorDecision)
            .HasConversion<string>()
            .HasDefaultValue(MentorDecision.Pending)
            .HasMaxLength(20);

        builder.Property(grp => grp.MentorComment).HasMaxLength(500);
        builder.Property(grp => grp.UpdatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(grp => grp.Group)
            .WithMany(g => g.GroupRoundProgresses)
            .HasForeignKey(grp => grp.GroupID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(grp => grp.ReviewRound)
            .WithMany(rr => rr.GroupRoundProgresses)
            .HasForeignKey(grp => grp.ReviewRoundID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

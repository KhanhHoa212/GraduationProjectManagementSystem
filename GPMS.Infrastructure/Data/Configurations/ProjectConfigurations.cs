using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("Semesters");
        builder.HasKey(s => s.SemesterID);
        builder.Property(s => s.SemesterCode).IsRequired().HasMaxLength(10).IsUnicode(false);
        builder.HasIndex(s => s.SemesterCode).IsUnique();
        builder.Property(s => s.AcademicYear).IsRequired().HasMaxLength(10);
        
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasDefaultValue(SemesterStatus.Upcoming)
            .HasMaxLength(20);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.ProjectID);
        builder.Property(p => p.ProjectCode).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.HasIndex(p => p.ProjectCode).IsUnique();
        builder.Property(p => p.ProjectName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");
        
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasDefaultValue(ProjectStatus.Draft)
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(p => p.Semester)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.SemesterID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Major)
            .WithMany(m => m.Projects)
            .HasForeignKey(p => p.MajorID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

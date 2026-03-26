using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("Faculties");
        builder.HasKey(f => f.FacultyID);
        builder.Property(f => f.FacultyCode).IsRequired().HasMaxLength(3).IsUnicode(false);
        builder.HasIndex(f => f.FacultyCode).IsUnique();
        builder.Property(f => f.FacultyName).IsRequired().HasMaxLength(100);
    }
}

public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        builder.ToTable("Majors");
        builder.HasKey(m => m.MajorID);
        builder.Property(m => m.MajorCode).IsRequired().HasMaxLength(3).IsUnicode(false);
        builder.HasIndex(m => m.MajorCode).IsUnique();
        builder.Property(m => m.MajorName).IsRequired().HasMaxLength(100);

        builder.HasOne(m => m.Faculty)
            .WithMany(f => f.Majors)
            .HasForeignKey(m => m.FacultyID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExpertiseAreaConfiguration : IEntityTypeConfiguration<ExpertiseArea>
{
    public void Configure(EntityTypeBuilder<ExpertiseArea> builder)
    {
        builder.ToTable("ExpertiseAreas");
        builder.HasKey(ea => ea.ExpertiseID);
        builder.Property(ea => ea.ExpertiseName).IsRequired().HasMaxLength(100);

        builder.HasOne(ea => ea.Major)
            .WithMany(m => m.ExpertiseAreas)
            .HasForeignKey(ea => ea.MajorID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class LecturerExpertiseConfiguration : IEntityTypeConfiguration<LecturerExpertise>
{
    public void Configure(EntityTypeBuilder<LecturerExpertise> builder)
    {
        builder.ToTable("LecturerExpertise");
        builder.HasKey(le => new { le.LecturerID, le.ExpertiseID });

        builder.Property(le => le.Level)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(le => le.IsPrimary).HasDefaultValue(false);

        builder.HasOne(le => le.Lecturer)
            .WithMany(u => u.LecturerExpertises)
            .HasForeignKey(le => le.LecturerID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(le => le.Expertise)
            .WithMany(e => e.LecturerExpertises)
            .HasForeignKey(le => le.ExpertiseID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

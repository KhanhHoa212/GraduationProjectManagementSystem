using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserID);
        builder.Property(u => u.UserID).HasMaxLength(20);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).HasMaxLength(100);
        builder.Property(u => u.Username).HasMaxLength(50);
        builder.Property(u => u.Phone).HasMaxLength(15);
        builder.Property(u => u.AvatarUrl).HasMaxLength(255);
        
        builder.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        builder.HasIndex(u => u.Username).IsUnique().HasFilter("[Username] IS NOT NULL");

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasDefaultValue(UserStatus.Active)
            .HasMaxLength(20);

        builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Check constraint: Email IS NOT NULL OR Username IS NOT NULL
        builder.ToTable(t => t.HasCheckConstraint("CK_User_EmailOrUsername", "[Email] IS NOT NULL OR [Username] IS NOT NULL"));
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(ur => ur.UserRoleID);
        
        builder.Property(ur => ur.RoleName)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ur => ur.AssignedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("UserCredentials");
        builder.HasKey(uc => uc.CredentialID);

        builder.Property(uc => uc.AuthProvider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(uc => uc.PasswordHash).HasMaxLength(255);
        builder.Property(uc => uc.ExternalProviderID).HasMaxLength(255);

        builder.HasIndex(uc => new { uc.UserID, uc.AuthProvider }).IsUnique();

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserCredentials)
            .HasForeignKey(uc => uc.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

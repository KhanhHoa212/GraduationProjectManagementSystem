using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GPMS.Infrastructure.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.RoomID);
        builder.Property(r => r.RoomCode).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.HasIndex(r => r.RoomCode).IsUnique();
        builder.Property(r => r.Building).HasMaxLength(100);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.Property(r => r.RoomType)
            .HasConversion<string>()
            .HasDefaultValue(RoomType.Classroom)
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasDefaultValue(RoomStatus.Available)
            .HasMaxLength(20);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.NotificationID);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(n => n.RelatedEntityType).HasMaxLength(50);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.IsEmailSent).HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(n => n.Recipient)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.RecipientID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

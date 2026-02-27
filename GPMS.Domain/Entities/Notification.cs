using GPMS.Domain.Enums;

namespace GPMS.Domain.Entities;

public class Notification
{
    public int NotificationID { get; set; }
    public string RecipientID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityID { get; set; }
    public bool IsRead { get; set; } = false;
    public bool IsEmailSent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public virtual User Recipient { get; set; } = null!;
}

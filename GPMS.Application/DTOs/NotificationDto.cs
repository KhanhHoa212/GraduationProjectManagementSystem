using GPMS.Domain.Enums;
using System;

namespace GPMS.Application.DTOs;

public class NotificationDto
{
    public int NotificationID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityID { get; set; }
}

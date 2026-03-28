using GPMS.Application.DTOs;
using System.Collections.Generic;

namespace GPMS.Web.ViewModels;

public class NotificationsViewModel
{
    public IEnumerable<NotificationDto> AllNotifications { get; set; } = new List<NotificationDto>();
    public IEnumerable<NotificationDto> DeadlineNotifications { get; set; } = new List<NotificationDto>();
    public IEnumerable<NotificationDto> FeedbackNotifications { get; set; } = new List<NotificationDto>();
    public int UnreadCount { get; set; }
    public string ActiveTab { get; set; } = "All";
    public Dictionary<string, List<NotificationDto>> GroupedNotifications { get; set; } = new();
}

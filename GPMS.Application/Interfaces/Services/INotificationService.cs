using GPMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientAsync(string userId, string? type = null);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task ToggleReadStatusAsync(int notificationId);
    Task<int> GetUnreadCountAsync(string userId);
}

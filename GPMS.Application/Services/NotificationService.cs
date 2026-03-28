using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsByRecipientAsync(string userId, string? type = null)
    {
        var notifications = await _notificationRepository.GetByRecipientAsync(userId);

        if (!string.IsNullOrEmpty(type) && type != "All")
        {
            if (Enum.TryParse<NotificationType>(type, true, out var filterType))
            {
                notifications = notifications.Where(n => n.Type == filterType);
            }
        }

        return notifications.OrderByDescending(n => n.CreatedAt).Select(n => new NotificationDto
        {
            NotificationID = n.NotificationID,
            Title = n.Title,
            Content = n.Content,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            RelatedEntityType = n.RelatedEntityType,
            RelatedEntityID = n.RelatedEntityID
        });
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task ToggleReadStatusAsync(int notificationId)
    {
        await _notificationRepository.ToggleReadStatusAsync(notificationId);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var notifications = await _notificationRepository.GetByRecipientAsync(userId);
        return notifications.Count(n => !n.IsRead);
    }
}

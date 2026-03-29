using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByRecipientAsync(string userId);
    Task<IEnumerable<Notification>> GetRecentByRecipientAsync(string userId, int count);
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task ToggleReadStatusAsync(int notificationId);
    Task<bool> HasNotificationAsync(string recipientId, GPMS.Domain.Enums.NotificationType type, int relatedEntityId, string titleKeyword, System.Threading.CancellationToken cancellationToken = default);
    Task<bool> HasSessionNotificationAsync(string recipientId, int sessionId, string titleKeyword, System.Threading.CancellationToken cancellationToken = default);
    Task SaveChangesAsync();
}

using GPMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByRecipientAsync(string userId);
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
    Task SaveChangesAsync();
}

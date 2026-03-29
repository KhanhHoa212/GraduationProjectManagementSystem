using GPMS.Domain.Entities;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly GpmsDbContext _context;
    public NotificationRepository(GpmsDbContext context) => _context = context;

    public async Task<IEnumerable<Notification>> GetByRecipientAsync(string userId) => 
        await _context.Notifications.Where(n => n.RecipientID == userId).ToListAsync();
        
    public async Task<IEnumerable<Notification>> GetRecentByRecipientAsync(string userId, int count) =>
        await _context.Notifications
            .Where(n => n.RecipientID == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task AddAsync(Notification notification) => await _context.Notifications.AddAsync(notification);
    public async Task MarkAsReadAsync(int notificationId)
    {
        var n = await _context.Notifications.FindAsync(notificationId);
        if (n != null) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.RecipientID == userId && !n.IsRead)
            .ToListAsync();
            
        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
    }
    
    public async Task ToggleReadStatusAsync(int notificationId)
    {
        var n = await _context.Notifications.FindAsync(notificationId);
        if (n != null)
        {
            n.IsRead = !n.IsRead;
            n.ReadAt = n.IsRead ? DateTime.UtcNow : null;
        }
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<bool> HasNotificationAsync(string recipientId, GPMS.Domain.Enums.NotificationType type, int relatedEntityId, string titleKeyword, System.Threading.CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AnyAsync(n => n.RecipientID == recipientId
                        && n.Type == type
                        && n.RelatedEntityID == relatedEntityId
                        && n.Title.Contains(titleKeyword), cancellationToken);
    }

    public async Task<bool> HasSessionNotificationAsync(string recipientId, int sessionId, string titleKeyword, System.Threading.CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AnyAsync(n => n.RecipientID == recipientId
                        && n.Type == GPMS.Domain.Enums.NotificationType.Schedule
                        && n.RelatedEntityType == "ReviewSession"
                        && n.RelatedEntityID == sessionId
                        && n.Title.Contains(titleKeyword), cancellationToken);
    }
}

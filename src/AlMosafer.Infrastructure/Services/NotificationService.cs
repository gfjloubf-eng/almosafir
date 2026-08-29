using AlMosafer.Application.DTOs.Notifications;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AlMosaferDbContext _dbContext;

    public NotificationService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendNotificationAsync(int userId, string title, string message, NotificationType type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
    {
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title ?? string.Empty,
            Message = n.Message ?? string.Empty,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationId);
        if (notification == null || notification.UserId != userId)
        {
            return false; // IDOR Protection
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}

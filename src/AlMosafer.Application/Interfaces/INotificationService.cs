using AlMosafer.Application.DTOs.Notifications;
using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(int userId, string title, string message, NotificationType type);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
}

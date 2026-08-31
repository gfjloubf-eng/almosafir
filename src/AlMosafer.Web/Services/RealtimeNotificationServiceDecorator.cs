using AlMosafer.Application.DTOs.Notifications;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Services;
using AlMosafer.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AlMosafer.Web.Services;

/// <summary>
/// مُزخرِف (Decorator) يلتفّ حول <see cref="NotificationService"/> دون تعديله:
/// بعد كل إشعار محفوظ في القاعدة يبثّ نسخة لحظية عبر SignalR إلى متصفح صاحبه
/// فيظهر كتنبيه فوري + تحديث لشارة الجرس — دون تحديث الصفحة.
///
/// لماذا مُزخرِف؟ لأن NotificationService يعيش في طبقة Infrastructure
/// (حيادي عن الويب — Clean Architecture)، واختباراته تبنيه بـ new مباشرةً؛
/// فأي تعديل على مُنشئه يكسر المعماريّة والاختبارات معاً. التغليف هنا
/// (طبقة Web) يضيف السلوك الجديد بصفر تعديل في القديم.
///
/// قاعدة صارمة: فشل البث لا يُسقط حفظ الإشعار أبداً (اللحظية تحسين لا شرط).
/// </summary>
public class RealtimeNotificationServiceDecorator : INotificationService
{
    private readonly NotificationService _inner;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly ILogger<RealtimeNotificationServiceDecorator> _logger;

    public RealtimeNotificationServiceDecorator(
        NotificationService inner,
        IHubContext<AppHub> hubContext,
        ILogger<RealtimeNotificationServiceDecorator> logger)
    {
        _inner = inner;
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        => _inner.GetUserNotificationsAsync(userId);

    public Task<int> GetUnreadCountAsync(int userId)
        => _inner.GetUnreadCountAsync(userId);

    public Task<bool> MarkAsReadAsync(int userId, int notificationId)
        => _inner.MarkAsReadAsync(userId, notificationId);

    public async Task SendNotificationAsync(int userId, string title, string message, NotificationType type)
    {
        // أولاً: الحقيقة الدائمة — صف الإشعار في القاعدة (كما كان دائماً)
        await _inner.SendNotificationAsync(userId, title, message, type);

        // ثانياً: النبضة اللحظية — إن تعثّرت تبقى القاعدة مصدر الحقيقة
        try
        {
            var unreadCount = await _inner.GetUnreadCountAsync(userId);
            await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type = type.ToString(),
                unreadCount,
                createdAtUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل بث الإشعار اللحظي للمستخدم {UserId} — الإشعار محفوظ في القاعدة", userId);
        }
    }
}

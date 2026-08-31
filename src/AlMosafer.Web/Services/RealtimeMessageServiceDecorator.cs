using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.Interfaces;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using AlMosafer.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Web.Services;

/// <summary>
/// النبض الحي — الموجة ٢: مزخرف الدردشة اللحظية.
/// يلتفّ حول <see cref="MessageService"/> دون تعديله: بعد حفظ الرسالة في القاعدة
/// يبثّها لحظياً إلى قناتين:
///   • مجموعة conv-{id}  → من فتح صفحة المحادثة يرى الرسالة فوراً (استبدال آمن بمحتوى الخادم).
///   • مجموعة user-{id}  → المستلم غير المتواجد في الصفحة يتلقى تنبيهاً عائماً.
/// الحفظ يبقى مصدر الحقيقة؛ أي عطب بثٍّ لا يُسقط الرسالة (تحسين لا شرط).
/// </summary>
public class RealtimeMessageServiceDecorator : IMessageService
{
    private readonly MessageService _inner;
    private readonly AlMosaferDbContext _db;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly ILogger<RealtimeMessageServiceDecorator> _logger;

    public RealtimeMessageServiceDecorator(
        MessageService inner,
        AlMosaferDbContext db,
        IHubContext<AppHub> hubContext,
        ILogger<RealtimeMessageServiceDecorator> logger)
    {
        _inner = inner;
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(int userId, int conversationId)
        => _inner.GetConversationMessagesAsync(userId, conversationId);

    public async Task<(bool Success, string Message, int? MessageId)> SendMessageAsync(int currentUserId, SendMessageDto dto)
    {
        var result = await _inner.SendMessageAsync(currentUserId, dto);
        if (!result.Success || !result.MessageId.HasValue)
        {
            return result;
        }

        try
        {
            var conversation = await _db.Conversations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == dto.ConversationId);
            if (conversation == null)
            {
                return result;
            }

            var recipientId = conversation.DriverId == currentUserId
                ? conversation.TravelerId
                : conversation.DriverId;
            var senderName = (await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId))?.Name ?? "مستخدم";
            var preview = dto.Content.Length > 80 ? dto.Content[..80] + "…" : dto.Content;

            var payload = new
            {
                messageId = result.MessageId.Value,
                conversationId = dto.ConversationId,
                senderId = currentUserId,
                senderName,
                preview,
                sentAtUtc = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"conv-{dto.ConversationId}").SendAsync("ReceiveMessage", payload);
            await _hubContext.Clients.Group($"user-{recipientId}").SendAsync("ReceiveMessage", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل بث الرسالة اللحظية للمحادثة {ConversationId} — الرسالة محفوظة في القاعدة", dto.ConversationId);
        }

        return result;
    }
}

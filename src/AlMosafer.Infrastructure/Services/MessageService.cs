using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly AlMosaferDbContext _dbContext;

    public MessageService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(int userId, int conversationId)
    {
        var conversation = await _dbContext.Conversations.FindAsync(conversationId);
        if (conversation == null || (conversation.DriverId != userId && conversation.TravelerId != userId))
        {
            return Enumerable.Empty<MessageDto>(); // IDOR Protection
        }

        var messages = await _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => new MessageDto
        {
            MessageId = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender.Name,
            Content = m.Text,
            IsRead = m.IsRead,
            SentAt = m.CreatedAt,
            IsMine = m.SenderId == userId
        });
    }

    public async Task<(bool Success, string Message, int? MessageId)> SendMessageAsync(int currentUserId, SendMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return (false, "محتوى الرسالة لا يمكن أن يكون فارغاً.", null);
        }

        var conversation = await _dbContext.Conversations.FindAsync(dto.ConversationId);
        if (conversation == null)
        {
            return (false, "المحادثة غير موجودة.", null);
        }

        // Ownership Check: Current user MUST be participant of conversation
        if (conversation.DriverId != currentUserId && conversation.TravelerId != currentUserId)
        {
            return (false, "لا تملك الصلاحية لإرسال رسائل في هذه المحادثة.", null);
        }

        var message = new Message
        {
            ConversationId = dto.ConversationId,
            SenderId = currentUserId,
            Text = dto.Content.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);

        // Update conversation last message timestamp
        conversation.LastMessageAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return (true, "تم إرسال الرسالة بنجاح.", message.Id);
    }
}

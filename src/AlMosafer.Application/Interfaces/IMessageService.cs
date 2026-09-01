using AlMosafer.Application.DTOs.Messaging;

namespace AlMosafer.Application.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(int userId, int conversationId);
    Task<(bool Success, string Message, int? MessageId)> SendMessageAsync(int currentUserId, SendMessageDto dto);
}

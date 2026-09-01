using AlMosafer.Application.DTOs.Messaging;

namespace AlMosafer.Application.Interfaces;

public interface IConversationService
{
    Task EnsureBookingConversationExistsAsync(int bookingId, int tripId, int driverId, int travelerId);
    Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId);
    Task<ConversationDto?> GetConversationByIdAsync(int userId, int conversationId);
}

namespace AlMosafer.Application.Interfaces;

public interface IResourceOwnershipService
{
    Task<bool> CanDriverModifyTripAsync(int driverId, int tripId);
    Task<bool> CanUserAccessBookingAsync(int userId, int bookingId);
    Task<bool> CanUserAccessConversationAsync(int userId, int conversationId);
}

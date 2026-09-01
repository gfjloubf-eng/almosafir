using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly AlMosaferDbContext _dbContext;

    public ConversationService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureBookingConversationExistsAsync(int bookingId, int tripId, int driverId, int travelerId)
    {
        var exists = await _dbContext.Conversations.AnyAsync(c => c.BookingId == bookingId);
        if (!exists)
        {
            var conversation = new Conversation
            {
                BookingId = bookingId,
                TripId = tripId,
                DriverId = driverId,
                TravelerId = travelerId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId)
    {
        var conversations = await _dbContext.Conversations
            .AsNoTracking()
            .Include(c => c.Trip)
            .Include(c => c.Driver)
            .Include(c => c.Traveler)
            .Include(c => c.Messages)
            .Where(c => c.DriverId == userId || c.TravelerId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();

        return conversations.Select(c =>
        {
            var isDriver = c.DriverId == userId;
            var otherUser = isDriver ? c.Traveler : c.Driver;
            var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()?.Text;

            return new ConversationDto
            {
                ConversationId = c.Id,
                BookingId = c.BookingId ?? 0,
                TripId = c.TripId,
                TripRoute = c.Trip != null ? $"{c.Trip.FromCity} ← {c.Trip.ToCity}" : "رحلة غير محددة",
                OtherUserId = otherUser?.Id ?? 0,
                OtherUserName = otherUser?.Name ?? "مستخدم",
                OtherUserRole = isDriver ? "مسافر" : "سائق",
                LastMessage = lastMsg,
                LastMessageAt = c.LastMessageAt ?? c.CreatedAt
            };
        });
    }

    public async Task<ConversationDto?> GetConversationByIdAsync(int userId, int conversationId)
    {
        var c = await _dbContext.Conversations
            .AsNoTracking()
            .Include(c => c.Trip)
            .Include(c => c.Driver)
            .Include(c => c.Traveler)
            .FirstOrDefaultAsync(conv => conv.Id == conversationId);

        if (c == null) return null;

        // Ownership Guard
        if (c.DriverId != userId && c.TravelerId != userId)
        {
            return null; // IDOR Protection
        }

        var isDriver = c.DriverId == userId;
        var otherUser = isDriver ? c.Traveler : c.Driver;

        return new ConversationDto
        {
            ConversationId = c.Id,
            BookingId = c.BookingId ?? 0,
            TripId = c.TripId,
            TripRoute = c.Trip != null ? $"{c.Trip.FromCity} ← {c.Trip.ToCity}" : "رحلة غير محددة",
            OtherUserId = otherUser?.Id ?? 0,
            OtherUserName = otherUser?.Name ?? "مستخدم",
            OtherUserRole = isDriver ? "مسافر" : "سائق",
            LastMessageAt = c.LastMessageAt ?? c.CreatedAt
        };
    }
}

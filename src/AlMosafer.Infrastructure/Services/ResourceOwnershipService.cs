using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class ResourceOwnershipService : IResourceOwnershipService
{
    private readonly AlMosaferDbContext _dbContext;

    public ResourceOwnershipService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CanDriverModifyTripAsync(int driverId, int tripId)
    {
        var user = await _dbContext.Users.FindAsync(driverId);
        if (user != null && user.Role == UserRole.Admin)
        {
            return true; // Admins can manage any trip
        }

        return await _dbContext.Trips.AnyAsync(t => t.Id == tripId && t.DriverId == driverId);
    }

    public async Task<bool> CanUserAccessBookingAsync(int userId, int bookingId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user != null && user.Role == UserRole.Admin)
        {
            return true; // Admins can view any booking
        }

        var booking = await _dbContext.Bookings
            .Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return false;

        // Either the traveler who made the booking or the driver of the trip
        return booking.TravelerId == userId || booking.Trip.DriverId == userId;
    }

    public async Task<bool> CanUserAccessConversationAsync(int userId, int conversationId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user != null && user.Role == UserRole.Admin)
        {
            return true;
        }

        return await _dbContext.Conversations
            .AnyAsync(c => c.Id == conversationId && (c.DriverId == userId || c.TravelerId == userId));
    }
}

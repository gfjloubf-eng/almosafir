using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.DTOs.Dashboard;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AlMosaferDbContext _dbContext;

    public DashboardService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TravelerDashboardDto> GetTravelerDashboardAsync(int travelerId)
    {
        var bookingsQuery = _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.TravelerId == travelerId);

        var totalBookings = await bookingsQuery.CountAsync();
        var activeBookings = await bookingsQuery.CountAsync(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded));
        var completedBookings = await bookingsQuery.CountAsync(b => b.Status == BookingStatus.Cancelled);

        var unreadNotifications = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == travelerId && !n.IsRead);

        var conversationsCount = await _dbContext.Conversations
            .AsNoTracking()
            .CountAsync(c => c.TravelerId == travelerId);

        var recentBookingsEntities = await _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
            .Where(b => b.TravelerId == travelerId)
            .OrderByDescending(b => b.BookingTime)
            .Take(5)
            .ToListAsync();

        var recentBookings = recentBookingsEntities.Select(b => new BookingDetailsDto
        {
            BookingId = b.Id,
            TripId = b.TripId,
            TravelerId = b.TravelerId,
            TravelerName = b.Traveler?.Name ?? "مسافر",
            TravelerPhone = b.Traveler?.Phone,
            DriverName = b.Trip?.Driver?.Name ?? "سائق",
            DriverPhone = b.Trip?.Driver?.Phone,
            FromCity = b.Trip?.FromCity ?? string.Empty,
            ToCity = b.Trip?.ToCity ?? string.Empty,
            TripTime = b.Trip?.TripTime ?? DateTime.MinValue,
            SeatsBooked = b.SeatsBooked,
            PricePerSeat = b.Trip?.PricePerSeat ?? 0,
            TotalAmount = (b.Trip?.PricePerSeat ?? 0) * b.SeatsBooked,
            Status = b.Status,
            TripStatus = b.Trip?.Status ?? TripStatus.Open,
            BookingTime = b.BookingTime
        });

        return new TravelerDashboardDto
        {
            TotalBookings = totalBookings,
            ActiveBookings = activeBookings,
            CompletedBookings = completedBookings,
            UnreadNotificationsCount = unreadNotifications,
            ConversationsCount = conversationsCount,
            RecentBookings = recentBookings
        };
    }

    public async Task<DriverDashboardDto> GetDriverDashboardAsync(int driverId)
    {
        var tripsQuery = _dbContext.Trips
            .AsNoTracking()
            .Where(t => t.DriverId == driverId);

        var totalTrips = await tripsQuery.CountAsync();
        var activeTrips = await tripsQuery.CountAsync(t => t.Status == TripStatus.Open);

        var totalSeatsBooked = await _dbContext.Bookings
            .AsNoTracking()
            .Where(b => b.Trip.DriverId == driverId && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded))
            .SumAsync(b => (int?)b.SeatsBooked) ?? 0;

        var totalEarnings = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Booking.Trip.DriverId == driverId && p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount) ?? 0.00m;

        var unreadNotifications = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == driverId && !n.IsRead);

        var conversationsCount = await _dbContext.Conversations
            .AsNoTracking()
            .CountAsync(c => c.DriverId == driverId);

        var recentTripsEntities = await _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .Where(t => t.DriverId == driverId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentTrips = recentTripsEntities.Select(t =>
        {
            var bookedSeatsCount = t.Bookings.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => b.SeatsBooked);
            var availableSeats = Math.Max(0, t.Seats - bookedSeatsCount);

            return new TripDetailsDto
            {
                Id = t.Id,
                DriverId = t.DriverId,
                DriverName = t.Driver?.Name ?? "سائق",
                DriverPhone = t.Driver?.Phone,
                DriverRating = t.Driver?.Rating ?? 0.0f,
                VehicleModel = t.Driver?.VehicleModel ?? t.VehicleInfo,
                PlateNumber = t.Driver?.PlateNumber,
                FromCity = t.FromCity,
                FromLocation = t.FromLocation ?? string.Empty,
                ToCity = t.ToCity,
                TripTime = t.TripTime,
                TotalSeats = t.Seats,
                AvailableSeats = availableSeats,
                PricePerSeat = t.PricePerSeat,
                Description = t.Description,
                VehicleInfo = t.VehicleInfo,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            };
        });

        return new DriverDashboardDto
        {
            TotalTrips = totalTrips,
            ActiveTrips = activeTrips,
            TotalSeatsBooked = totalSeatsBooked,
            TotalEarnings = totalEarnings,
            UnreadNotificationsCount = unreadNotifications,
            ConversationsCount = conversationsCount,
            RecentTrips = recentTrips
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(int adminUserId)
    {
        var totalUsers = await _dbContext.Users.AsNoTracking().CountAsync();
        var travelersCount = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Traveler);
        var driversCount = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Driver);

        var totalTrips = await _dbContext.Trips.AsNoTracking().CountAsync();
        var activeTrips = await _dbContext.Trips.AsNoTracking().CountAsync(t => t.Status == TripStatus.Open);

        var totalBookings = await _dbContext.Bookings.AsNoTracking().CountAsync();

        var totalPaymentsAmount = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => (decimal?)p.Amount) ?? 0.00m;

        var unreadNotifications = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == adminUserId && !n.IsRead);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TravelersCount = travelersCount,
            DriversCount = driversCount,
            TotalTrips = totalTrips,
            ActiveTrips = activeTrips,
            TotalBookings = totalBookings,
            TotalPaymentsAmount = totalPaymentsAmount,
            UnreadNotificationsCount = unreadNotifications
        };
    }
}

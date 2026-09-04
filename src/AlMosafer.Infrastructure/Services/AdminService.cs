using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.DTOs.Dashboard;
using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.DTOs.Notifications;
using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AlMosaferDbContext _dbContext;
    private readonly IDbConnectionHealthService _dbHealthService;

    public AdminService(AlMosaferDbContext dbContext, IDbConnectionHealthService dbHealthService)
    {
        _dbContext = dbContext;
        _dbHealthService = dbHealthService;
    }

    public async Task<IEnumerable<UserProfileDto>> GetUsersAsync(string? search = null, UserRole? roleFilter = null)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(cleanSearch) || 
                                     u.Email.ToLower().Contains(cleanSearch) || 
                                     (u.Phone != null && u.Phone.Contains(cleanSearch)));
        }

        if (roleFilter.HasValue)
        {
            query = query.Where(u => u.Role == roleFilter.Value);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                City = u.City,
                PlateNumber = u.PlateNumber,
                VehicleModel = u.VehicleModel,
                VehicleYear = u.VehicleYear,
                Rating = u.Rating,
                TotalTrips = u.TotalTrips,
                TotalEarnings = u.TotalEarnings,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserProfileDto?> GetUserDetailsAsync(int userId)
    {
        var u = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        if (u == null) return null;

        return new UserProfileDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            City = u.City,
            PlateNumber = u.PlateNumber,
            VehicleModel = u.VehicleModel,
            VehicleYear = u.VehicleYear,
            Rating = u.Rating,
            TotalTrips = u.TotalTrips,
            TotalEarnings = u.TotalEarnings,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<IEnumerable<TripDetailsDto>> GetTripsAsync(string? origin = null, string? destination = null, int? driverId = null, TripStatus? statusFilter = null)
    {
        var query = _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(origin))
        {
            query = query.Where(t => t.FromCity.ToLower().Contains(origin.Trim().ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(t => t.ToCity.ToLower().Contains(destination.Trim().ToLower()));
        }

        if (driverId.HasValue)
        {
            query = query.Where(t => t.DriverId == driverId.Value);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(t => t.Status == statusFilter.Value);
        }

        var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return list.Select(t =>
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
    }

    public async Task<IEnumerable<BookingDetailsDto>> GetBookingsAsync(BookingStatus? statusFilter = null)
    {
        var query = _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Traveler)
            .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(b => b.Status == statusFilter.Value);
        }

        return await query
            .OrderByDescending(b => b.BookingTime)
            .Select(b => new BookingDetailsDto
            {
                BookingId = b.Id,
                TripId = b.TripId,
                TravelerId = b.TravelerId,
                TravelerName = b.Traveler != null ? b.Traveler.Name : "مسافر",
                TravelerPhone = b.Traveler != null ? b.Traveler.Phone : null,
                DriverName = b.Trip != null && b.Trip.Driver != null ? b.Trip.Driver.Name : "سائق",
                DriverPhone = b.Trip != null && b.Trip.Driver != null ? b.Trip.Driver.Phone : null,
                FromCity = b.Trip != null ? b.Trip.FromCity : string.Empty,
                ToCity = b.Trip != null ? b.Trip.ToCity : string.Empty,
                TripTime = b.Trip != null ? b.Trip.TripTime : DateTime.MinValue,
                SeatsBooked = b.SeatsBooked,
                PricePerSeat = b.Trip != null ? b.Trip.PricePerSeat : 0,
                TotalAmount = (b.Trip != null ? b.Trip.PricePerSeat : 0) * b.SeatsBooked,
                Status = b.Status,
                TripStatus = b.Trip != null ? b.Trip.Status : TripStatus.Open,
                BookingTime = b.BookingTime
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentDetailsDto>> GetPaymentsAsync()
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Traveler)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Trip)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentDetailsDto
            {
                PaymentId = p.Id,
                BookingId = p.BookingId,
                TravelerName = p.Booking != null && p.Booking.Traveler != null ? p.Booking.Traveler.Name : "مسافر",
                Amount = p.Amount,
                Status = p.Status,
                TransactionId = p.TransactionId ?? string.Empty,
                CreatedAt = p.CreatedAt,
                TripRoute = p.Booking != null && p.Booking.Trip != null ? $"{p.Booking.Trip.FromCity} ← {p.Booking.Trip.ToCity}" : "غير محدد"
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<RatingDetailsDto>> GetRatingsAsync()
    {
        return await _dbContext.Ratings
            .AsNoTracking()
            .Include(r => r.Traveler)
            .Include(r => r.Driver)
            .Include(r => r.Trip)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingDetailsDto
            {
                Id = r.Id,
                TripId = r.TripId,
                TravelerId = r.TravelerId,
                TravelerName = r.Traveler != null ? r.Traveler.Name : "مسافر",
                DriverId = r.DriverId,
                DriverName = r.Driver != null ? r.Driver.Name : "سائق",
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                FromCity = r.Trip != null ? r.Trip.FromCity : string.Empty,
                ToCity = r.Trip != null ? r.Trip.ToCity : string.Empty
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync()
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message ?? string.Empty,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync()
    {
        var rawConversations = await _dbContext.Conversations
            .AsNoTracking()
            .Include(c => c.Traveler)
            .Include(c => c.Driver)
            .Include(c => c.Trip)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return rawConversations.Select(c =>
        {
            var travelerName = c.Traveler?.Name ?? "مسافر";
            var driverName = c.Driver?.Name ?? "سائق";
            var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

            return new ConversationDto
            {
                ConversationId = c.Id,
                BookingId = c.BookingId ?? 0,
                TripId = c.TripId,
                TripRoute = c.Trip != null ? $"{c.Trip.FromCity} ← {c.Trip.ToCity}" : "رحلة غير محددة",
                OtherUserId = c.DriverId,
                OtherUserName = $"{travelerName} ↔ {driverName}",
                OtherUserRole = "محادثة راكب وسائق",
                LastMessage = lastMsg?.Text ?? "بدء المحادثة",
                LastMessageAt = lastMsg?.CreatedAt ?? c.CreatedAt
            };
        });
    }

    public async Task<AdminSystemHealthDto> GetSystemHealthAsync()
    {
        var healthCheck = await _dbHealthService.CheckConnectionAsync();
        return new AdminSystemHealthDto
        {
            IsDatabaseConnected = healthCheck.CanConnect,
            DatabaseProvider = "MySQL / MariaDB (XAMPP Server Environment)",
            ApplicationStatus = healthCheck.CanConnect ? "Healthy / Operational" : "Warning / Unreachable",
            EnvironmentName = "Development",
            RuntimeVersion = ".NET 10.0",
            SupportManagerName = "عمار عادل المصوعي",
            SupportManagerPhone = "712275038",
            CheckedAt = DateTime.UtcNow
        };
    }
}

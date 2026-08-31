using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly AlMosaferDbContext _dbContext;

    public RatingService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool Success, string Message, RatingDetailsDto? Rating)> CreateRatingAsync(int travelerId, CreateRatingDto dto)
    {
        // 1. Server-side score validation (1 to 5 stars)
        if (dto.Value < 1 || dto.Value > 5)
        {
            return (false, "قيمة التقييم غير صالحة. يجب أن تكون بين 1 و 5 نجوم.", null);
        }

        // 2. Fetch Trip and Driver
        var trip = await _dbContext.Trips
            .Include(t => t.Driver)
            .FirstOrDefaultAsync(t => t.Id == dto.TripId);

        if (trip == null)
        {
            return (false, "الرحلة المطلوبة غير موجودة في النظام.", null);
        }

        // 3. Guard against rating oneself
        if (travelerId == trip.DriverId)
        {
            return (false, "لا يمكنك تقييم نفسك كرئيس للرحلة أو سائق.", null);
        }

        // 4. Verify Traveler has a valid confirmed booking for this trip
        var hasBooking = await _dbContext.Bookings
            .AnyAsync(b => b.TripId == dto.TripId && b.TravelerId == travelerId && b.Status == BookingStatus.Confirmed);

        if (!hasBooking)
        {
            return (false, "لا يمكنك تقييم رحلة لم تقم بحجزها أو لم تتأكد حجوزاتك فيها.", null);
        }

        // 5. Duplicate Rating Guard (Prevent rating same trip multiple times)
        var alreadyRated = await _dbContext.Ratings
            .AnyAsync(r => r.TripId == dto.TripId && r.TravelerId == travelerId);

        if (alreadyRated)
        {
            return (false, "لقد قمت بتقييم هذه الرحلة مسبقاً ولا يمكنك تقييمها مرة أخرى.", null);
        }

        // 6. Create Rating Record
        var traveler = await _dbContext.Users.FindAsync(travelerId);

        var ratingEntity = new Rating
        {
            TripId = dto.TripId,
            TravelerId = travelerId,
            DriverId = trip.DriverId,
            Value = dto.Value,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Ratings.Add(ratingEntity);
        await _dbContext.SaveChangesAsync();

        // 7. Recalculate Driver Average Rating dynamically from DB
        var driverRatings = await _dbContext.Ratings
            .Where(r => r.DriverId == trip.DriverId)
            .ToListAsync();

        if (driverRatings.Any())
        {
            var newAvgRating = (float)Math.Round(driverRatings.Average(r => r.Value), 1);
            var driverUser = await _dbContext.Users.FindAsync(trip.DriverId);
            if (driverUser != null)
            {
                driverUser.Rating = newAvgRating;
                await _dbContext.SaveChangesAsync();
            }
        }

        var resultDto = new RatingDetailsDto
        {
            Id = ratingEntity.Id,
            TripId = ratingEntity.TripId,
            TravelerId = ratingEntity.TravelerId,
            TravelerName = traveler?.Name ?? "مسافر",
            DriverId = ratingEntity.DriverId,
            DriverName = trip.Driver?.Name ?? "سائق",
            Value = ratingEntity.Value,
            Comment = ratingEntity.Comment,
            CreatedAt = ratingEntity.CreatedAt,
            FromCity = trip.FromCity,
            ToCity = trip.ToCity
        };

        return (true, "تم إرسال تقييمك للسائق بنجاح! شكراً لمشاركتك.", resultDto);
    }

    public async Task<DriverRatingSummaryDto> GetDriverRatingSummaryAsync(int driverId)
    {
        var driver = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == driverId);
        if (driver == null)
        {
            return new DriverRatingSummaryDto { DriverId = driverId, AverageRating = 0, TotalRatingsCount = 0 };
        }

        var ratingsQuery = _dbContext.Ratings
            .AsNoTracking()
            .Include(r => r.Traveler)
            .Include(r => r.Trip)
            .Where(r => r.DriverId == driverId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await ratingsQuery.CountAsync();
        var avgRating = totalCount > 0 
            ? (float)Math.Round(await ratingsQuery.AverageAsync(r => r.Value), 1) 
            : 0.0f;

        var recentRatings = await ratingsQuery.Take(10).Select(r => new RatingDetailsDto
        {
            Id = r.Id,
            TripId = r.TripId,
            TravelerId = r.TravelerId,
            TravelerName = r.Traveler != null ? r.Traveler.Name : "مسافر",
            DriverId = r.DriverId,
            DriverName = driver.Name,
            Value = r.Value,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            FromCity = r.Trip != null ? r.Trip.FromCity : string.Empty,
            ToCity = r.Trip != null ? r.Trip.ToCity : string.Empty
        }).ToListAsync();

        return new DriverRatingSummaryDto
        {
            DriverId = driverId,
            DriverName = driver.Name,
            AverageRating = avgRating,
            TotalRatingsCount = totalCount,
            RecentRatings = recentRatings
        };
    }

    public async Task<IEnumerable<RatingDetailsDto>> GetTripRatingsAsync(int tripId)
    {
        return await _dbContext.Ratings
            .AsNoTracking()
            .Include(r => r.Traveler)
            .Include(r => r.Driver)
            .Include(r => r.Trip)
            .Where(r => r.TripId == tripId)
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

    public async Task<bool> HasTravelerRatedTripAsync(int travelerId, int tripId)
    {
        return await _dbContext.Ratings
            .AsNoTracking()
            .AnyAsync(r => r.TravelerId == travelerId && r.TripId == tripId);
    }

    public async Task<bool> CanTravelerRateBookingAsync(int travelerId, int bookingId)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.TravelerId == travelerId);

        if (booking == null || booking.Status != BookingStatus.Confirmed)
        {
            return false;
        }

        var alreadyRated = await _dbContext.Ratings
            .AsNoTracking()
            .AnyAsync(r => r.TravelerId == travelerId && r.TripId == booking.TripId);

        return !alreadyRated;
    }
}

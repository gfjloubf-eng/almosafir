using AlMosafer.Application.DTOs.Reports;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private readonly AlMosaferDbContext _dbContext;

    public ReportingService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminReportSummaryDto> GetAdminReportSummaryAsync(ReportFilterDto filter)
    {
        var sanitizedFilter = SanitizeFilter(filter);

        var summary = new AdminReportSummaryDto
        {
            ActiveFilter = sanitizedFilter,
            UserStats = await CalculateUserStatisticsAsync(sanitizedFilter),
            TripStats = await CalculateTripStatisticsAsync(sanitizedFilter),
            BookingStats = await CalculateBookingStatisticsAsync(sanitizedFilter),
            PaymentStats = await CalculatePaymentStatisticsAsync(sanitizedFilter),
            RatingStats = await CalculateRatingStatisticsAsync(sanitizedFilter),
            PopularRoutes = await CalculatePopularRoutesAsync(sanitizedFilter),
            TopDrivers = await CalculateDriverPerformanceAsync(sanitizedFilter),
            BookingTrend = await CalculateBookingTrendAsync(sanitizedFilter)
        };

        return summary;
    }

    private ReportFilterDto SanitizeFilter(ReportFilterDto filter)
    {
        var sanitized = new ReportFilterDto
        {
            RoleFilter = filter.RoleFilter
        };

        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate > filter.ToDate)
        {
            // Invalid date range: swap dates
            sanitized.FromDate = filter.ToDate.Value.Date;
            sanitized.ToDate = filter.FromDate.Value.Date.AddDays(1).AddTicks(-1);
        }
        else
        {
            if (filter.FromDate.HasValue)
                sanitized.FromDate = filter.FromDate.Value.Date;

            if (filter.ToDate.HasValue)
                sanitized.ToDate = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
        }

        return sanitized;
    }

    private async Task<UserStatisticsDto> CalculateUserStatisticsAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(u => u.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(u => u.CreatedAt <= filter.ToDate.Value);
        if (filter.RoleFilter.HasValue)
            query = query.Where(u => u.Role == filter.RoleFilter.Value);

        var users = await query.Select(u => u.Role).ToListAsync();

        return new UserStatisticsDto
        {
            TotalUsers = users.Count,
            TravelersCount = users.Count(r => r == UserRole.Traveler),
            DriversCount = users.Count(r => r == UserRole.Driver),
            AdminsCount = users.Count(r => r == UserRole.Admin)
        };
    }

    private async Task<TripStatisticsDto> CalculateTripStatisticsAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Trips.AsNoTracking().Include(t => t.Bookings).AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

        var trips = await query.ToListAsync();

        if (!trips.Any()) return new TripStatisticsDto();

        var topOrigin = trips.GroupBy(t => t.FromCity).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "غير محدد";
        var topDest = trips.GroupBy(t => t.ToCity).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "غير محدد";

        var totalSeatsBooked = trips.Sum(t => t.Bookings.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => b.SeatsBooked));

        return new TripStatisticsDto
        {
            TotalTrips = trips.Count,
            ActiveTrips = trips.Count(t => t.Status == TripStatus.Open),
            CompletedTrips = trips.Count(t => t.Status == TripStatus.Completed),
            CancelledTrips = trips.Count(t => t.Status == TripStatus.Cancelled),
            AveragePricePerSeat = trips.Any() ? Math.Round(trips.Average(t => t.PricePerSeat), 2) : 0,
            AverageSeatsPerTrip = trips.Any() ? Math.Round(trips.Average(t => t.Seats), 1) : 0,
            TotalSeats = trips.Sum(t => t.Seats),
            TotalSeatsBooked = totalSeatsBooked,
            TopOriginCity = topOrigin,
            TopDestinationCity = topDest
        };
    }

    private async Task<BookingStatisticsDto> CalculateBookingStatisticsAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Bookings.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(b => b.BookingTime >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(b => b.BookingTime <= filter.ToDate.Value);

        var bookings = await query.ToListAsync();

        if (!bookings.Any()) return new BookingStatisticsDto();

        var totalSeatsBooked = bookings.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => b.SeatsBooked);

        return new BookingStatisticsDto
        {
            TotalBookings = bookings.Count,
            ConfirmedBookings = bookings.Count(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)),
            CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
            PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
            AverageSeatsPerBooking = bookings.Any() ? Math.Round(bookings.Average(b => b.SeatsBooked), 1) : 0,
            TotalSeatsBooked = totalSeatsBooked
        };
    }

    private async Task<PaymentStatisticsDto> CalculatePaymentStatisticsAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Payments.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= filter.ToDate.Value);

        var payments = await query.ToListAsync();

        if (!payments.Any()) return new PaymentStatisticsDto();

        var paidList = payments.Where(p => p.Status == PaymentStatus.Paid).ToList();

        return new PaymentStatisticsDto
        {
            TotalTransactionsCount = payments.Count,
            TotalPaidAmount = paidList.Sum(p => p.Amount),
            PaidTransactionsCount = paidList.Count,
            PendingTransactionsCount = payments.Count(p => p.Status == PaymentStatus.Pending),
            FailedTransactionsCount = payments.Count(p => p.Status == PaymentStatus.Failed)
        };
    }

    private async Task<RatingStatisticsDto> CalculateRatingStatisticsAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Ratings.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);

        var ratings = await query.ToListAsync();

        if (!ratings.Any()) return new RatingStatisticsDto();

        return new RatingStatisticsDto
        {
            TotalRatingsCount = ratings.Count,
            AverageRating = Math.Round(ratings.Average(r => r.Value), 2),
            FiveStarCount = ratings.Count(r => r.Value == 5),
            FourStarCount = ratings.Count(r => r.Value == 4),
            ThreeStarCount = ratings.Count(r => r.Value == 3),
            TwoStarCount = ratings.Count(r => r.Value == 2),
            OneStarCount = ratings.Count(r => r.Value == 1)
        };
    }

    private async Task<List<RouteStatisticsDto>> CalculatePopularRoutesAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Trips.AsNoTracking().Include(t => t.Bookings).AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

        var trips = await query.ToListAsync();

        if (!trips.Any()) return new List<RouteStatisticsDto>();

        var routeGroups = trips.GroupBy(t => new { t.FromCity, t.ToCity })
            .Select(g =>
            {
                var confirmedBookings = g.SelectMany(t => t.Bookings).Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).ToList();
                var seatsCount = confirmedBookings.Sum(b => b.SeatsBooked);
                var revenue = g.Sum(t => t.PricePerSeat * t.Bookings.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => b.SeatsBooked));

                return new RouteStatisticsDto
                {
                    FromCity = g.Key.FromCity,
                    ToCity = g.Key.ToCity,
                    TripsCount = g.Count(),
                    BookingsCount = confirmedBookings.Count,
                    SeatsBookedCount = seatsCount,
                    TotalRevenue = revenue
                };
            })
            .OrderByDescending(r => r.BookingsCount)
            .ThenByDescending(r => r.TripsCount)
            .Take(10)
            .ToList();

        return routeGroups;
    }

    public async Task<IEnumerable<DriverPerformanceDto>> GetTopDriversAsync(int count = 4)
    {
        var performances = await CalculateDriverPerformanceAsync(new ReportFilterDto());
        return performances
            .OrderByDescending(d => d.AverageRating)
            .ThenByDescending(d => d.TripsCount)
            .Take(count)
            .ToList();
    }

    private async Task<List<DriverPerformanceDto>> CalculateDriverPerformanceAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

        var trips = await query.ToListAsync();

        if (!trips.Any()) return new List<DriverPerformanceDto>();

        var driverGroups = trips.GroupBy(t => t.DriverId)
            .Select(g =>
            {
                var driver = g.First().Driver;
                var confirmedBookings = g.SelectMany(t => t.Bookings).Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).ToList();
                var seatsCount = confirmedBookings.Sum(b => b.SeatsBooked);
                var earnings = g.Sum(t => t.PricePerSeat * t.Bookings.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => b.SeatsBooked));

                return new DriverPerformanceDto
                {
                    DriverId = g.Key,
                    DriverName = driver?.Name ?? $"سائق #{g.Key}",
                    DriverPhone = driver?.Phone,
                    TripsCount = g.Count(),
                    BookingsCount = confirmedBookings.Count,
                    SeatsBookedCount = seatsCount,
                    AverageRating = Math.Round(driver?.Rating ?? 0.0, 1),
                    TotalEarnings = earnings
                };
            })
            .OrderByDescending(d => d.TripsCount)
            .ThenByDescending(d => d.BookingsCount)
            .Take(10)
            .ToList();

        return driverGroups;
    }

    private async Task<List<TimeSeriesPointDto>> CalculateBookingTrendAsync(ReportFilterDto filter)
    {
        var query = _dbContext.Bookings.AsNoTracking().Include(b => b.Trip).AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(b => b.BookingTime >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(b => b.BookingTime <= filter.ToDate.Value);

        var bookings = await query.ToListAsync();

        if (!bookings.Any()) return new List<TimeSeriesPointDto>();

        var trend = bookings.GroupBy(b => b.BookingTime.Date)
            .Select(g => new TimeSeriesPointDto
            {
                Date = g.Key,
                PeriodLabel = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count(),
                Amount = g.Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded)).Sum(b => (b.Trip?.PricePerSeat ?? 0) * b.SeatsBooked)
            })
            .OrderBy(pt => pt.Date)
            .Take(30)
            .ToList();

        return trend;
    }
}

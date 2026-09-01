using AlMosafer.Application.DTOs.Reports;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class ReportingServiceTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetSummary_ReturnsRealDatabaseMetrics()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var admin = new User { Name = "مدير المنظومة", Email = "admin@test.com", Role = UserRole.Admin };
        var traveler = new User { Name = "مسافر طارق", Email = "tareq@test.com", Role = UserRole.Traveler };
        var driver = new User { Name = "سائق عبده", Email = "abdo@test.com", Role = UserRole.Driver };
        dbContext.Users.AddRange(admin, traveler, driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "صنعاء", ToCity = "عدن", PricePerSeat = 5000, Seats = 4, Status = TripStatus.Open };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 2, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(3, summary.UserStats.TotalUsers);
        Assert.Equal(1, summary.UserStats.TravelersCount);
        Assert.Equal(1, summary.UserStats.DriversCount);
        Assert.Equal(1, summary.TripStats.TotalTrips);
        Assert.Equal(1, summary.BookingStats.TotalBookings);
    }

    [Fact]
    public async Task GetUserStatistics_ReturnsCorrectRoleCounts()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        dbContext.Users.AddRange(
            new User { Name = "مسافر 1", Email = "t1@test.com", Role = UserRole.Traveler },
            new User { Name = "مسافر 2", Email = "t2@test.com", Role = UserRole.Traveler },
            new User { Name = "سائق 1", Email = "d1@test.com", Role = UserRole.Driver }
        );
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(3, summary.UserStats.TotalUsers);
        Assert.Equal(2, summary.UserStats.TravelersCount);
        Assert.Equal(1, summary.UserStats.DriversCount);
        Assert.Equal(66.7, summary.UserStats.TravelersPercentage);
    }

    [Fact]
    public async Task GetTripStatistics_ReturnsCorrectCounts()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var driver = new User { Name = "سائق صالح", Email = "saleh@driver.com", Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        dbContext.Trips.AddRange(
            new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "صنعاء", Status = TripStatus.Open, PricePerSeat = 4000, Seats = 4 },
            new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "إب", Status = TripStatus.Completed, PricePerSeat = 2000, Seats = 4 }
        );
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(2, summary.TripStats.TotalTrips);
        Assert.Equal(1, summary.TripStats.ActiveTrips);
        Assert.Equal(1, summary.TripStats.CompletedTrips);
        Assert.Equal(3000, summary.TripStats.AveragePricePerSeat);
    }

    [Fact]
    public async Task GetBookingStatistics_ReturnsCorrectStatusCounts()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var driver = new User { Name = "سائق ماجد", Email = "majed@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر وليد", Email = "waleed@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "عدن", ToCity = "تعز" };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.AddRange(
            new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 1, Status = BookingStatus.Confirmed },
            new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 2, Status = BookingStatus.Cancelled }
        );
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(2, summary.BookingStats.TotalBookings);
        Assert.Equal(1, summary.BookingStats.ConfirmedBookings);
        Assert.Equal(1, summary.BookingStats.CancelledBookings);
        Assert.Equal(50.0, summary.BookingStats.ConfirmationRate);
    }

    [Fact]
    public async Task GetPaymentStatistics_ReturnsCorrectTotals()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var booking1 = new Booking { TravelerId = 1, Status = BookingStatus.Confirmed };
        var booking2 = new Booking { TravelerId = 2, Status = BookingStatus.Confirmed };
        dbContext.Bookings.AddRange(booking1, booking2);
        await dbContext.SaveChangesAsync();

        dbContext.Payments.AddRange(
            new Payment { BookingId = booking1.Id, Amount = 10000, Status = PaymentStatus.Paid, TransactionId = "TX-1" },
            new Payment { BookingId = booking2.Id, Amount = 5000, Status = PaymentStatus.Paid, TransactionId = "TX-2" }
        );
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(2, summary.PaymentStats.TotalTransactionsCount);
        Assert.Equal(15000, summary.PaymentStats.TotalPaidAmount);
        Assert.Equal(7500, summary.PaymentStats.AverageTransactionAmount);
    }

    [Fact]
    public async Task GetRatingStatistics_ReturnsCorrectAverage()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        dbContext.Ratings.AddRange(
            new Rating { TripId = 1, TravelerId = 1, DriverId = 2, Value = 5 },
            new Rating { TripId = 1, TravelerId = 2, DriverId = 2, Value = 3 }
        );
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Equal(2, summary.RatingStats.TotalRatingsCount);
        Assert.Equal(4.0, summary.RatingStats.AverageRating);
        Assert.Equal(1, summary.RatingStats.FiveStarCount);
        Assert.Equal(1, summary.RatingStats.ThreeStarCount);
    }

    [Fact]
    public async Task GetRouteStatistics_ReturnsRealGroupedRoutes()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var driver = new User { Name = "سائق هادي", Email = "hadi@driver.com", Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "صنعاء", ToCity = "إب", PricePerSeat = 3000 };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.Add(new Booking { TripId = trip.Id, TravelerId = 1, SeatsBooked = 2, Status = BookingStatus.Confirmed });
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Single(summary.PopularRoutes);
        Assert.Equal("صنعاء ← إب", summary.PopularRoutes.First().RouteName);
        Assert.Equal(6000, summary.PopularRoutes.First().TotalRevenue);
    }

    [Fact]
    public async Task GetDriverPerformance_ReturnsRealData()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var driver = new User { Name = "سائق عدنان", Email = "adnan@driver.com", Role = UserRole.Driver, Phone = "771122334", Rating = 4.8f };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "المكلا", ToCity = "سيئون", PricePerSeat = 4000 };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.Add(new Booking { TripId = trip.Id, TravelerId = 1, SeatsBooked = 3, Status = BookingStatus.Confirmed });
        await dbContext.SaveChangesAsync();

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.Single(summary.TopDrivers);
        Assert.Equal("سائق عدنان", summary.TopDrivers.First().DriverName);
        Assert.Equal(12000, summary.TopDrivers.First().TotalEarnings);
    }

    [Fact]
    public async Task DateFilter_ReturnsOnlyDataWithinRange()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var oldUser = new User { Name = "قديم", Email = "old@test.com", Role = UserRole.Traveler, CreatedAt = new DateTime(2025, 1, 1) };
        var newUser = new User { Name = "جديد", Email = "new@test.com", Role = UserRole.Traveler, CreatedAt = new DateTime(2026, 6, 1) };
        dbContext.Users.AddRange(oldUser, newUser);
        await dbContext.SaveChangesAsync();

        var filter = new ReportFilterDto
        {
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 12, 31)
        };

        var summary = await reportingService.GetAdminReportSummaryAsync(filter);

        Assert.Equal(1, summary.UserStats.TotalUsers);
    }

    [Fact]
    public async Task InvalidDateRange_IsRejectedAndSwappedSafely()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var filter = new ReportFilterDto
        {
            FromDate = new DateTime(2026, 12, 31),
            ToDate = new DateTime(2026, 1, 1)
        };

        var summary = await reportingService.GetAdminReportSummaryAsync(filter);

        Assert.NotNull(summary);
        Assert.True(summary.ActiveFilter.FromDate <= summary.ActiveFilter.ToDate);
    }

    [Fact]
    public async Task EmptyDatabase_DoesNotCrashAndReturnsZeroMetrics()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var reportingService = new ReportingService(dbContext);

        var summary = await reportingService.GetAdminReportSummaryAsync(new ReportFilterDto());

        Assert.NotNull(summary);
        Assert.Equal(0, summary.UserStats.TotalUsers);
        Assert.Equal(0, summary.TripStats.TotalTrips);
        Assert.Equal(0, summary.BookingStats.TotalBookings);
        Assert.Equal(0, summary.PaymentStats.TotalTransactionsCount);
        Assert.Equal(0, summary.RatingStats.TotalRatingsCount);
        Assert.Empty(summary.PopularRoutes);
        Assert.Empty(summary.TopDrivers);
    }
}

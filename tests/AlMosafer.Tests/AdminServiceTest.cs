using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace AlMosafer.Tests;

public class AdminServiceTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private class TestDbHealthService : IDbConnectionHealthService
    {
        public Task<(bool CanConnect, string Message, string DatabaseName)> CheckConnectionAsync()
        {
            return Task.FromResult((true, "Connected", "mosafir_db"));
        }
    }

    [Fact]
    public async Task AdminDashboard_ReturnsRealMetrics()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var dashboardService = new DashboardService(dbContext);

        var admin = new User { Name = "مدير النظام", Email = "admin@test.com", Role = UserRole.Admin };
        var traveler = new User { Name = "مسافر أيمن", Email = "ayman@test.com", Role = UserRole.Traveler };
        var driver = new User { Name = "سائق سالم", Email = "salem@test.com", Role = UserRole.Driver };
        dbContext.Users.AddRange(admin, traveler, driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "صنعاء", Status = TripStatus.Open };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var metrics = await dashboardService.GetAdminDashboardAsync(admin.Id);

        Assert.Equal(3, metrics.TotalUsers);
        Assert.Equal(1, metrics.TravelersCount);
        Assert.Equal(1, metrics.DriversCount);
        Assert.Equal(1, metrics.TotalTrips);
        Assert.Equal(1, metrics.ActiveTrips);
    }

    [Fact]
    public async Task AdminUsers_ReturnsRealUsers()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        dbContext.Users.AddRange(
            new User { Name = "مستخدم 1", Email = "u1@test.com", Role = UserRole.Traveler },
            new User { Name = "مستخدم 2", Email = "u2@test.com", Role = UserRole.Driver }
        );
        await dbContext.SaveChangesAsync();

        var users = await adminService.GetUsersAsync();

        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task AdminUsers_SearchFilter_ReturnsFilteredResults()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        dbContext.Users.AddRange(
            new User { Name = "سامي القدسي", Email = "sami@test.com", Role = UserRole.Traveler },
            new User { Name = "مراد الضالعي", Email = "murad@test.com", Role = UserRole.Driver }
        );
        await dbContext.SaveChangesAsync();

        var filtered = await adminService.GetUsersAsync(search: "سامي");

        Assert.Single(filtered);
        Assert.Equal("سامي القدسي", filtered.First().Name);
    }

    [Fact]
    public async Task AdminTripList_ReturnsRealTrips()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        var driver = new User { Name = "سائق توفيق", Email = "tawfiq@driver.com", Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        dbContext.Trips.Add(new Trip { DriverId = driver.Id, FromCity = "عدن", ToCity = "المكلا", Status = TripStatus.Open });
        await dbContext.SaveChangesAsync();

        var trips = await adminService.GetTripsAsync();

        Assert.Single(trips);
        Assert.Equal("عدن", trips.First().FromCity);
        Assert.Equal("المكلا", trips.First().ToCity);
    }

    [Fact]
    public async Task AdminBookingList_ReturnsRealBookings()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        var driver = new User { Name = "سائق نبيل", Email = "nabil@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر حامد", Email = "hamed@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "إب", PricePerSeat = 3000 };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.Add(new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 2, Status = BookingStatus.Confirmed });
        await dbContext.SaveChangesAsync();

        var bookings = await adminService.GetBookingsAsync();

        Assert.Single(bookings);
        Assert.Equal(6000, bookings.First().TotalAmount);
    }

    [Fact]
    public async Task AdminPaymentList_ReturnsRealPayments()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        var driver = new User { Name = "سائق سالم", Email = "salem@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر جابر", Email = "jaber@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "عدن" };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        dbContext.Payments.Add(new Payment { BookingId = booking.Id, Amount = 10000, Status = PaymentStatus.Paid, TransactionId = "TXN-999" });
        await dbContext.SaveChangesAsync();

        var payments = await adminService.GetPaymentsAsync();

        Assert.Single(payments);
        Assert.Equal("TXN-999", payments.First().TransactionId);
        Assert.Equal(10000, payments.First().Amount);
    }

    [Fact]
    public async Task AdminRatings_ReturnsRealRatings()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        var driver = new User { Name = "سائق فؤاد", Email = "fouad@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر زياد", Email = "ziad@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "مأرب", ToCity = "صنعاء" };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Ratings.Add(new Rating { TripId = trip.Id, TravelerId = traveler.Id, DriverId = driver.Id, Value = 5, Comment = "ممتاز" });
        await dbContext.SaveChangesAsync();

        var ratings = await adminService.GetRatingsAsync();

        Assert.Single(ratings);
        Assert.Equal(5, ratings.First().Value);
    }

    [Fact]
    public async Task AdminSystemHealth_ReturnsDatabaseStatus()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var adminService = new AdminService(dbContext, new TestDbHealthService());

        var health = await adminService.GetSystemHealthAsync();

        Assert.True(health.IsDatabaseConnected);
        Assert.Equal("عمار عادل المصوعي", health.SupportManagerName);
        Assert.Equal("712275038", health.SupportManagerPhone);
    }
}

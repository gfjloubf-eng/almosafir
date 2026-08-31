using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class DashboardServiceTest
{
    private AlMosaferDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AlMosaferDbContext(options);
    }

    [Fact]
    public async Task TravelerDashboard_ReturnsRealBookingCount()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var service = new DashboardService(db);

        var traveler = new User { Id = 10, Name = "Traveler Test", Role = UserRole.Traveler };
        var driver = new User { Id = 20, Name = "Driver Test", Role = UserRole.Driver };
        db.Users.AddRange(traveler, driver);

        var trip = new Trip { Id = 1, DriverId = 20, FromCity = "Sanaa", ToCity = "Aden", Seats = 4, PricePerSeat = 10000, Status = TripStatus.Open };
        db.Trips.Add(trip);

        var booking = new Booking { Id = 100, TripId = 1, TravelerId = 10, SeatsBooked = 2, Status = BookingStatus.Confirmed };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        // Act
        var dashboard = await service.GetTravelerDashboardAsync(10);

        // Assert
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.TotalBookings);
        Assert.Equal(1, dashboard.ActiveBookings);
        Assert.Single(dashboard.RecentBookings);
        Assert.Equal("Sanaa", dashboard.RecentBookings.First().FromCity);
    }

    [Fact]
    public async Task DriverDashboard_ReturnsRealTripCountAndEarnings()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var service = new DashboardService(db);

        var driver = new User { Id = 30, Name = "Driver Hero", Role = UserRole.Driver };
        db.Users.Add(driver);

        var trip = new Trip { Id = 5, DriverId = 30, FromCity = "Taiz", ToCity = "Ibb", Seats = 3, PricePerSeat = 5000, Status = TripStatus.Open };
        db.Trips.Add(trip);

        var booking = new Booking { Id = 200, TripId = 5, TravelerId = 10, SeatsBooked = 2, Status = BookingStatus.Confirmed };
        db.Bookings.Add(booking);

        var payment = new Payment { Id = 50, BookingId = 200, Amount = 10000, Status = PaymentStatus.Paid, TransactionId = "TXN-99" };
        db.Payments.Add(payment);

        await db.SaveChangesAsync();

        // Act
        var dashboard = await service.GetDriverDashboardAsync(30);

        // Assert
        Assert.NotNull(dashboard);
        Assert.Equal(1, dashboard.TotalTrips);
        Assert.Equal(1, dashboard.ActiveTrips);
        Assert.Equal(2, dashboard.TotalSeatsBooked);
        Assert.Equal(10000m, dashboard.TotalEarnings);
    }

    [Fact]
    public async Task AdminDashboard_ReturnsRealUserCount()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var service = new DashboardService(db);

        db.Users.Add(new User { Id = 1, Name = "Admin User", Role = UserRole.Admin });
        db.Users.Add(new User { Id = 2, Name = "Traveler A", Role = UserRole.Traveler });
        db.Users.Add(new User { Id = 3, Name = "Driver B", Role = UserRole.Driver });
        await db.SaveChangesAsync();

        // Act
        var dashboard = await service.GetAdminDashboardAsync(1);

        // Assert
        Assert.NotNull(dashboard);
        Assert.Equal(3, dashboard.TotalUsers);
        Assert.Equal(1, dashboard.TravelersCount);
        Assert.Equal(1, dashboard.DriversCount);
    }
}

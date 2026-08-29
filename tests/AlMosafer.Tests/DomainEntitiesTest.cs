using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class DomainEntitiesTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CanAddUserAndTrip_AndQueryRelationships()
    {
        var options = CreateInMemoryOptions();

        using (var context = new AlMosaferDbContext(options))
        {
            var driver = new User
            {
                Name = "علي السائق",
                Email = "ali@driver.com",
                PasswordHash = "hashedpassword123",
                Role = UserRole.Driver,
                City = "تعز"
            };

            context.Users.Add(driver);
            await context.SaveChangesAsync();

            var trip = new Trip
            {
                DriverId = driver.Id,
                FromCity = "تعز",
                FromLocation = "الحوبان",
                ToCity = "عدن",
                TripTime = DateTime.UtcNow.AddDays(1),
                Seats = 4,
                PricePerSeat = 15000.00m,
                Status = TripStatus.Open
            };

            context.Trips.Add(trip);
            await context.SaveChangesAsync();
        }

        using (var context = new AlMosaferDbContext(options))
        {
            var tripFromDb = await context.Trips
                .Include(t => t.Driver)
                .FirstOrDefaultAsync();

            Assert.NotNull(tripFromDb);
            Assert.Equal("تعز", tripFromDb.FromCity);
            Assert.Equal("عدن", tripFromDb.ToCity);
            Assert.Equal("علي السائق", tripFromDb.Driver.Name);
            Assert.Equal(UserRole.Driver, tripFromDb.Driver.Role);
        }
    }

    [Fact]
    public async Task CanCreateBookingAndPayment_RelationshipCheck()
    {
        var options = CreateInMemoryOptions();

        using (var context = new AlMosaferDbContext(options))
        {
            var traveler = new User { Name = "محمد المسافر", Email = "mohammed@traveler.com", PasswordHash = "hash123" };
            var driver = new User { Name = "أحمد السائق", Email = "ahmed@driver.com", PasswordHash = "hash456" };
            
            context.Users.AddRange(traveler, driver);
            await context.SaveChangesAsync();

            var trip = new Trip { DriverId = driver.Id, FromCity = "صنعاء", ToCity = "إب", TripTime = DateTime.UtcNow.AddDays(2), Seats = 3, PricePerSeat = 10000.00m };
            context.Trips.Add(trip);
            await context.SaveChangesAsync();

            var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 2, Status = BookingStatus.Confirmed };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var payment = new Payment { BookingId = booking.Id, Amount = 20000.00m, Status = PaymentStatus.Paid, TransactionId = "TXN-9988" };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();
        }

        using (var context = new AlMosaferDbContext(options))
        {
            var bookingFromDb = await context.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Traveler)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync();

            Assert.NotNull(bookingFromDb);
            Assert.Equal(2, bookingFromDb.SeatsBooked);
            Assert.Equal("محمد المسافر", bookingFromDb.Traveler.Name);
            Assert.NotNull(bookingFromDb.Payment);
            Assert.Equal(20000.00m, bookingFromDb.Payment.Amount);
            Assert.Equal(PaymentStatus.Paid, bookingFromDb.Payment.Status);
        }
    }
}

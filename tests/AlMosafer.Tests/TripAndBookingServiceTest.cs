using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class TripAndBookingServiceTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private (BookingService bookingService, TripService tripService) CreateServices(AlMosaferDbContext dbContext)
    {
        var paymentService = new PaymentService(dbContext);
        var notificationService = new NotificationService(dbContext);
        var conversationService = new ConversationService(dbContext);
        var bookingService = new BookingService(dbContext, paymentService, notificationService, conversationService);
        var tripService = new TripService(dbContext);
        return (bookingService, tripService);
    }

    [Fact]
    public async Task CreateTrip_DriverUser_ReturnsSuccess()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "فؤاد السائق", Email = "fouad@driver.com", Role = UserRole.Driver, VehicleModel = "Hilux 2020", PlateNumber = "123-تعز" };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var (_, tripService) = CreateServices(dbContext);

        var dto = new CreateTripDto
        {
            FromCity = "صنعاء",
            ToCity = "عدن",
            TripTime = DateTime.Now.AddDays(2),
            Seats = 4,
            PricePerSeat = 15000.00m
        };

        var result = await tripService.CreateTripAsync(driver.Id, dto);

        Assert.True(result.Success);
        Assert.NotNull(result.TripId);

        var trip = await dbContext.Trips.FindAsync(result.TripId.Value);
        Assert.NotNull(trip);
        Assert.Equal("صنعاء", trip.FromCity);
        Assert.Equal("عدن", trip.ToCity);
        Assert.Equal(4, trip.Seats);
    }

    [Fact]
    public async Task CreateBooking_SeatAvailable_SuccessfullyBooksAndDecreasesSeats()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق أحمد", Email = "ahmed@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "المسافر مراد", Email = "morad@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip
        {
            DriverId = driver.Id,
            FromCity = "تعز",
            ToCity = "صنعاء",
            TripTime = DateTime.Now.AddDays(1),
            Seats = 4,
            PricePerSeat = 10000.00m,
            Status = TripStatus.Open
        };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var (bookingService, tripService) = CreateServices(dbContext);

        var bookingDto = new CreateBookingDto
        {
            TripId = trip.Id,
            SeatsBooked = 2
        };

        var bookingResult = await bookingService.CreateBookingAsync(traveler.Id, bookingDto);

        Assert.True(bookingResult.Success);
        Assert.NotNull(bookingResult.BookingId);

        // Verify remaining available seats via TripService
        var tripDetails = await tripService.GetTripByIdAsync(trip.Id);
        Assert.NotNull(tripDetails);
        Assert.Equal(4, tripDetails.TotalSeats);
        Assert.Equal(2, tripDetails.AvailableSeats); // 4 - 2 = 2

        // Verify payment record generated automatically
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingResult.BookingId.Value);
        Assert.NotNull(payment);
        Assert.Equal(20000.00m, payment.Amount); // 2 * 10000
        Assert.Equal(PaymentStatus.Paid, payment.Status);
    }

    [Fact]
    public async Task CreateBooking_ExceedsAvailableSeats_FailsOverbooking()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق أحمد", Email = "ahmed@driver.com", Role = UserRole.Driver };
        var travelerA = new User { Name = "مسافر 1", Email = "t1@traveler.com", Role = UserRole.Traveler };
        var travelerB = new User { Name = "مسافر 2", Email = "t2@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, travelerA, travelerB);
        await dbContext.SaveChangesAsync();

        var trip = new Trip
        {
            DriverId = driver.Id,
            FromCity = "تعز",
            ToCity = "عدن",
            TripTime = DateTime.Now.AddDays(1),
            Seats = 3,
            PricePerSeat = 10000.00m,
            Status = TripStatus.Open
        };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var (bookingService, _) = CreateServices(dbContext);

        // Traveler A books 2 seats
        await bookingService.CreateBookingAsync(travelerA.Id, new CreateBookingDto { TripId = trip.Id, SeatsBooked = 2 });

        // Traveler B tries to book 2 seats (only 1 available)
        var overbookingResult = await bookingService.CreateBookingAsync(travelerB.Id, new CreateBookingDto { TripId = trip.Id, SeatsBooked = 2 });

        Assert.False(overbookingResult.Success);
        Assert.Contains("المقاعد المتبقية في هذه الرحلة هي 1 مقعد فقط", overbookingResult.Message);
    }

    [Fact]
    public async Task CreateBooking_DuplicateBooking_ReturnsError()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق علي", Email = "ali@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر مح مح", Email = "mah@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip
        {
            DriverId = driver.Id,
            FromCity = "عدن",
            ToCity = "صنعاء",
            TripTime = DateTime.Now.AddDays(1),
            Seats = 4,
            PricePerSeat = 12000.00m,
            Status = TripStatus.Open
        };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var (bookingService, _) = CreateServices(dbContext);

        // First booking succeeds
        var firstBooking = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto { TripId = trip.Id, SeatsBooked = 1 });
        Assert.True(firstBooking.Success);

        // Duplicate booking attempt by same traveler fails
        var duplicateBooking = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto { TripId = trip.Id, SeatsBooked = 1 });
        Assert.False(duplicateBooking.Success);
        Assert.Contains("لديك حجز مؤكد سابق", duplicateBooking.Message);
    }

    [Fact]
    public async Task CreateBooking_DriverBookingOwnTrip_ReturnsError()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق حسن", Email = "hassan@driver.com", Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip
        {
            DriverId = driver.Id,
            FromCity = "إب",
            ToCity = "تعز",
            TripTime = DateTime.Now.AddDays(1),
            Seats = 4,
            PricePerSeat = 8000.00m,
            Status = TripStatus.Open
        };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var (bookingService, _) = CreateServices(dbContext);

        var ownBookingResult = await bookingService.CreateBookingAsync(driver.Id, new CreateBookingDto { TripId = trip.Id, SeatsBooked = 1 });
        Assert.False(ownBookingResult.Success);
        Assert.Contains("لا يمكنك حجز مقعد في رحلة تقوم أنت بقيادتها", ownBookingResult.Message);
    }


    private async Task<(int driverId, int tripId)> SeedOpenTrip(AlMosaferDbContext dbContext, string email)
    {
        var driver = new User { Name = "سائق تجريبي", Email = email, Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var (_, tripService) = CreateServices(dbContext);
        var result = await tripService.CreateTripAsync(driver.Id, new CreateTripDto
        {
            FromCity = "صنعاء",
            ToCity = "عدن",
            TripTime = DateTime.Now.AddDays(2),
            Seats = 3,
            PricePerSeat = 12000.00m
        });
        return (driver.Id, result.TripId!.Value);
    }

    [Fact]
    public async Task StartTrip_OwnerDriver_MarksTripAsStarted()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "owner1@test.com");
        var (_, tripService) = CreateServices(dbContext);

        var result = await tripService.StartTripAsync(driverId, tripId);

        Assert.True(result.Success);
        var trip = await dbContext.Trips.FindAsync(tripId);
        Assert.Equal(TripStatus.Started, trip!.Status);
    }

    [Fact]
    public async Task StartTrip_DifferentDriver_ReturnsForbidden()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (_, tripId) = await SeedOpenTrip(dbContext, "owner2@test.com");
        var stranger = new User { Name = "سائق آخر", Email = "stranger@test.com", Role = UserRole.Driver };
        dbContext.Users.Add(stranger);
        await dbContext.SaveChangesAsync();
        var (_, tripService) = CreateServices(dbContext);

        var result = await tripService.StartTripAsync(stranger.Id, tripId);

        Assert.False(result.Success);
        var trip = await dbContext.Trips.FindAsync(tripId);
        Assert.Equal(TripStatus.Open, trip!.Status);
    }

    [Fact]
    public async Task StartTrip_AlreadyStarted_ReturnsFailure()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "owner3@test.com");
        var (_, tripService) = CreateServices(dbContext);
        await tripService.StartTripAsync(driverId, tripId);

        var second = await tripService.StartTripAsync(driverId, tripId);

        Assert.False(second.Success);
    }

    [Fact]
    public async Task CompleteTrip_NotStarted_ReturnsFailure()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "owner4@test.com");
        var (_, tripService) = CreateServices(dbContext);

        var result = await tripService.CompleteTripAsync(driverId, tripId);

        Assert.False(result.Success);
        var trip = await dbContext.Trips.FindAsync(tripId);
        Assert.Equal(TripStatus.Open, trip!.Status);
    }

    [Fact]
    public async Task CompleteTrip_AfterStart_MarksTripAsCompleted()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "owner5@test.com");
        var (_, tripService) = CreateServices(dbContext);
        await tripService.StartTripAsync(driverId, tripId);

        var result = await tripService.CompleteTripAsync(driverId, tripId);

        Assert.True(result.Success);
        var trip = await dbContext.Trips.FindAsync(tripId);
        Assert.Equal(TripStatus.Completed, trip!.Status);
    }

    [Fact]
    public async Task CancelTrip_OwnerDriver_MarksTripAsCancelled()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "owner6@test.com");
        var (_, tripService) = CreateServices(dbContext);

        var result = await tripService.CancelTripAsync(driverId, tripId);

        Assert.True(result.Success);
        var trip = await dbContext.Trips.FindAsync(tripId);
        Assert.Equal(TripStatus.Cancelled, trip!.Status);
    }
}

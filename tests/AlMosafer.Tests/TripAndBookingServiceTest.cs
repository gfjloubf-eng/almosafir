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


    [Fact]
    public async Task WatchRoute_NewRoute_SavesToPreferences()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var user = new User { Name = "مسافر", Email = "w1@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var watchlist = new WatchlistService(dbContext, new NotificationService(dbContext));

        var result = await watchlist.WatchRouteAsync(user.Id, "صنعاء", "عدن");

        Assert.True(result.Success);
        var routes = await watchlist.GetWatchedRoutesAsync(user.Id);
        Assert.Single(routes);
        Assert.Equal("صنعاء", routes[0].FromCity);
    }

    [Fact]
    public async Task WatchRoute_Duplicate_ReturnsFailure()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var user = new User { Name = "مسافر", Email = "w2@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var watchlist = new WatchlistService(dbContext, new NotificationService(dbContext));
        await watchlist.WatchRouteAsync(user.Id, "صنعاء", "عدن");

        var second = await watchlist.WatchRouteAsync(user.Id, "صنعاء", "عدن");

        Assert.False(second.Success);
        Assert.Single(await watchlist.GetWatchedRoutesAsync(user.Id));
    }

    [Fact]
    public async Task NotifyWatchers_MatchingRoute_CreatesTripUpdateNotification()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var traveler = new User { Name = "مسافر", Email = "w3@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var watchlist = new WatchlistService(dbContext, new NotificationService(dbContext));
        await watchlist.WatchRouteAsync(traveler.Id, "صنعاء", "عدن");
        var (_, tripId) = await SeedOpenTrip(dbContext, "drive-w@test.com");

        var notified = await watchlist.NotifyWatchersForTripAsync(tripId);

        Assert.Equal(1, notified);
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.UserId == traveler.Id);
        Assert.NotNull(notification);
        Assert.Equal(NotificationType.TripUpdate, notification!.Type);
    }

    [Fact]
    public async Task NotifyWatchers_NonMatchingRoute_NotifiesNobody()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var traveler = new User { Name = "مسافر", Email = "w4@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var watchlist = new WatchlistService(dbContext, new NotificationService(dbContext));
        await watchlist.WatchRouteAsync(traveler.Id, "تعز", "الحديدة");
        var (_, tripId) = await SeedOpenTrip(dbContext, "drive-x@test.com");

        var notified = await watchlist.NotifyWatchersForTripAsync(tripId);

        Assert.Equal(0, notified);
        Assert.False(await dbContext.Notifications.AnyAsync(n => n.UserId == traveler.Id));
    }


    [Fact]
    public async Task CreateBooking_CashOption_PaymentStaysPending()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "cash1@test.com");
        var traveler = new User { Name = "مسافر", Email = "cash1t@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var (bookingService, _) = CreateServices(dbContext);

        var result = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto
        {
            TripId = tripId,
            SeatsBooked = 1,
            PaymentMethod = PaymentMethod.Cash
        });

        Assert.True(result.Success);
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.BookingId == result.BookingId!.Value);
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Pending, payment!.Status);
        Assert.Null(payment.TransactionId);
    }

    [Fact]
    public async Task CreateBooking_DefaultOption_MockGatewayMarksPaid()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "mock1@test.com");
        var traveler = new User { Name = "مسافر", Email = "mock1t@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var (bookingService, _) = CreateServices(dbContext);

        var result = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto
        {
            TripId = tripId,
            SeatsBooked = 1
        });

        Assert.True(result.Success);
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.BookingId == result.BookingId!.Value);
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Paid, payment!.Status);
        Assert.StartsWith("TXN-", payment.TransactionId);
    }

    [Fact]
    public async Task ConfirmCash_TripDriver_MarksPaymentPaid()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "cash2@test.com");
        var traveler = new User { Name = "مسافر", Email = "cash2t@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var (bookingService, _) = CreateServices(dbContext);
        var booking = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto { TripId = tripId, SeatsBooked = 1, PaymentMethod = PaymentMethod.Cash });
        var payment = await dbContext.Payments.FirstAsync(p => p.BookingId == booking.BookingId!.Value);
        var paymentService = new PaymentService(dbContext);

        var result = await paymentService.ConfirmCashReceivedAsync(driverId, payment.Id);

        Assert.True(result.Success);
        var reloaded = await dbContext.Payments.FindAsync(payment.Id);
        Assert.Equal(PaymentStatus.Paid, reloaded!.Status);
        Assert.StartsWith("CASH-", reloaded.TransactionId);
    }

    [Fact]
    public async Task ConfirmCash_StrangerUser_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var (driverId, tripId) = await SeedOpenTrip(dbContext, "cash3@test.com");
        var traveler = new User { Name = "مسافر", Email = "cash3t@test.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();
        var (bookingService, _) = CreateServices(dbContext);
        var booking = await bookingService.CreateBookingAsync(traveler.Id, new CreateBookingDto { TripId = tripId, SeatsBooked = 1, PaymentMethod = PaymentMethod.Cash });
        var payment = await dbContext.Payments.FirstAsync(p => p.BookingId == booking.BookingId!.Value);
        var paymentService = new PaymentService(dbContext);

        var result = await paymentService.ConfirmCashReceivedAsync(traveler.Id, payment.Id);

        Assert.False(result.Success);
        var reloaded = await dbContext.Payments.FindAsync(payment.Id);
        Assert.Equal(PaymentStatus.Pending, reloaded!.Status);
    }


    private async Task<int> SeedTrip(AlMosaferDbContext dbContext, string email, string from, string to)
    {
        var driver = new User { Name = "سائق", Email = email, Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();
        var (_, tripService) = CreateServices(dbContext);
        var result = await tripService.CreateTripAsync(driver.Id, new CreateTripDto
        {
            FromCity = from,
            ToCity = to,
            TripTime = DateTime.Now.AddDays(1),
            Seats = 4,
            PricePerSeat = 500.00m
        });
        return result.TripId!.Value;
    }

    [Fact]
    public async Task GetInternalLines_IncludesOnlySameCityTrips()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        await SeedTrip(dbContext, "line1@test.com", "صنعاء", "صنعاء");
        await SeedTrip(dbContext, "line2@test.com", "صنعاء", "عدن");
        var (_, tripService) = CreateServices(dbContext);

        var lines = (await tripService.GetInternalLinesAsync()).ToList();

        Assert.Single(lines);
        Assert.Equal("صنعاء", lines[0].FromCity);
        Assert.Equal("صنعاء", lines[0].ToCity);
    }

    [Fact]
    public async Task GetInternalLines_ExcludesCancelledAndNonOpen()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var tripId = await SeedTrip(dbContext, "line3@test.com", "عدن", "عدن");
        var (_, tripService) = CreateServices(dbContext);
        var trip = await dbContext.Trips.FindAsync(tripId);
        var driver = await dbContext.Users.FindAsync(trip!.DriverId);
        await tripService.CancelTripAsync(driver!.Id, tripId);

        var lines = await tripService.GetInternalLinesAsync();

        Assert.Empty(lines);
    }


    [Fact]
    public async Task CreateTrip_SameCity_AllowedAsInternalLine()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var tripId = await SeedTrip(dbContext, "samecity@test.com", "صنعاء", "صنعاء");

        var trip = await dbContext.Trips.FindAsync(tripId);

        Assert.NotNull(trip);
        Assert.Equal(trip!.FromCity, trip.ToCity);
    }


    [Fact]
    public async Task CreateLine_ThenListAndDetails_WithStopsAndSchedules()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var lineService = new LineService(dbContext);

        var created = await lineService.CreateLineAsync("خط شملان", "صنعاء", "خط تجريبي");
        Assert.True(created.Success);

        var all = (await lineService.GetActiveLinesAsync()).ToList();
        Assert.Single(all);

        var lineId = all[0].Id;
        await lineService.AddStopAsync(lineId, "باب شملان", 1);
        await lineService.AddStopAsync(lineId, "الجامعة", 2);
        await lineService.AddScheduleAsync(lineId, 6, "16:30");

        var details = await lineService.GetLineDetailsAsync(lineId);
        Assert.NotNull(details);
        Assert.Equal(2, details!.Stops.Count);
        Assert.Equal("باب شملان", details.Stops[0].Name);
        Assert.Single(details.Schedules);
        Assert.Equal("السبت", details.Schedules[0].DayName);
        Assert.Equal("16:30", details.Schedules[0].TimeText);
    }

    [Fact]
    public async Task GetActiveLines_ExcludesInactiveLines()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var lineService = new LineService(dbContext);
        await lineService.CreateLineAsync("خط أ", "عدن", null);
        var lineId = (await lineService.GetActiveLinesAsync()).First().Id;

        await lineService.SetLineActiveAsync(lineId, false);

        Assert.Empty(await lineService.GetActiveLinesAsync());
        Assert.Single(await lineService.GetAllLinesAsync());
    }

    [Fact]
    public async Task DeleteLine_RemovesStopsAndSchedulesCascade()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var lineService = new LineService(dbContext);
        await lineService.CreateLineAsync("خط ب", "تعز", null);
        var lineId = (await lineService.GetActiveLinesAsync()).First().Id;
        await lineService.AddStopAsync(lineId, "الحوبان", 1);
        await lineService.AddScheduleAsync(lineId, 0, "07:00");

        var result = await lineService.DeleteLineAsync(lineId);

        Assert.True(result.Success);
        Assert.Empty(await lineService.GetAllLinesAsync());
        Assert.Empty(dbContext.LineStops);
        Assert.Empty(dbContext.LineSchedules);
    }
}

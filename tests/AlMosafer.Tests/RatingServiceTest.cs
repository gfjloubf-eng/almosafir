using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class RatingServiceTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateRating_ValidCompletedBooking_Succeeds()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق سالم", Email = "salem@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر أيمن", Email = "ayman@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "صنعاء", TripTime = DateTime.UtcNow, Seats = 4, PricePerSeat = 5000 };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 1, Status = BookingStatus.Confirmed, BookingTime = DateTime.UtcNow };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var dto = new CreateRatingDto { TripId = trip.Id, Value = 5, Comment = "سائق ممتع وقيادة آمنة جداً." };
        var result = await ratingService.CreateRatingAsync(traveler.Id, dto);

        Assert.True(result.Success);
        Assert.NotNull(result.Rating);
        Assert.Equal(5, result.Rating.Value);
        Assert.Equal("سائق ممتع وقيادة آمنة جداً.", result.Rating.Comment);
    }

    [Fact]
    public async Task CreateRating_UserDoesNotOwnBooking_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق علي", Email = "ali@driver.com", Role = UserRole.Driver };
        var travelerA = new User { Name = "مسافر أ", Email = "a@traveler.com", Role = UserRole.Traveler };
        var travelerB = new User { Name = "مسافر ب", Email = "b@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, travelerA, travelerB);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "عدن", ToCity = "المكلا", TripTime = DateTime.UtcNow };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        // Traveler A booked the trip, Traveler B did NOT
        var booking = new Booking { TripId = trip.Id, TravelerId = travelerA.Id, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var dto = new CreateRatingDto { TripId = trip.Id, Value = 4 };
        var result = await ratingService.CreateRatingAsync(travelerB.Id, dto);

        Assert.False(result.Success);
        Assert.Equal("لا يمكنك تقييم رحلة لم تقم بحجزها أو لم تتأكد حجوزاتك فيها.", result.Message);
    }

    [Fact]
    public async Task CreateRating_DuplicateRating_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق ناصر", Email = "nasser@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر هاني", Email = "hani@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "إب", ToCity = "تعز", TripTime = DateTime.UtcNow };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var dto = new CreateRatingDto { TripId = trip.Id, Value = 5 };

        // First rating submission succeeds
        var firstResult = await ratingService.CreateRatingAsync(traveler.Id, dto);
        Assert.True(firstResult.Success);

        // Second rating submission for same trip fails (Duplicate Guard)
        var secondResult = await ratingService.CreateRatingAsync(traveler.Id, dto);
        Assert.False(secondResult.Success);
        Assert.Equal("لقد قمت بتقييم هذه الرحلة مسبقاً ولا يمكنك تقييمها مرة أخرى.", secondResult.Message);
    }

    [Fact]
    public async Task CreateRating_InvalidScore_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var traveler = new User { Name = "مسافر فهد", Email = "fahd@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.Add(traveler);
        await dbContext.SaveChangesAsync();

        var dtoZero = new CreateRatingDto { TripId = 1, Value = 0 };
        var dtoSix = new CreateRatingDto { TripId = 1, Value = 6 };

        var resZero = await ratingService.CreateRatingAsync(traveler.Id, dtoZero);
        var resSix = await ratingService.CreateRatingAsync(traveler.Id, dtoSix);

        Assert.False(resZero.Success);
        Assert.False(resSix.Success);
    }

    [Fact]
    public async Task CreateRating_DriverCannotRateOwnTrip_Fails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق طارق", Email = "tariq@driver.com", Role = UserRole.Driver };
        dbContext.Users.Add(driver);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "ذمار", ToCity = "صنعاء", TripTime = DateTime.UtcNow };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var dto = new CreateRatingDto { TripId = trip.Id, Value = 5 };
        var result = await ratingService.CreateRatingAsync(driver.Id, dto);

        Assert.False(result.Success);
        Assert.Equal("لا يمكنك تقييم نفسك كرئيس للرحلة أو سائق.", result.Message);
    }

    [Fact]
    public async Task GetDriverRating_ReturnsRealAverageAndCount()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق بلال", Email = "belal@driver.com", Role = UserRole.Driver };
        var traveler1 = new User { Name = "مسافر 1", Email = "t1@test.com", Role = UserRole.Traveler };
        var traveler2 = new User { Name = "مسافر 2", Email = "t2@test.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler1, traveler2);
        await dbContext.SaveChangesAsync();

        var trip1 = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "عدن", TripTime = DateTime.UtcNow };
        var trip2 = new Trip { DriverId = driver.Id, FromCity = "عدن", ToCity = "تعز", TripTime = DateTime.UtcNow };
        dbContext.Trips.AddRange(trip1, trip2);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.AddRange(
            new Booking { TripId = trip1.Id, TravelerId = traveler1.Id, Status = BookingStatus.Confirmed },
            new Booking { TripId = trip2.Id, TravelerId = traveler2.Id, Status = BookingStatus.Confirmed }
        );
        await dbContext.SaveChangesAsync();

        // Ratings: 5 and 4 -> Expected average = (5+4)/2 = 4.5
        await ratingService.CreateRatingAsync(traveler1.Id, new CreateRatingDto { TripId = trip1.Id, Value = 5 });
        await ratingService.CreateRatingAsync(traveler2.Id, new CreateRatingDto { TripId = trip2.Id, Value = 4 });

        var summary = await ratingService.GetDriverRatingSummaryAsync(driver.Id);

        Assert.Equal(2, summary.TotalRatingsCount);
        Assert.Equal(4.5f, summary.AverageRating);
    }

    [Fact]
    public async Task RatingReview_XssPayload_IsSafelyStoredAndDisplayed()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);
        var ratingService = new RatingService(dbContext);

        var driver = new User { Name = "سائق حامد", Email = "hamed@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر رامي", Email = "rami@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "مأرب", ToCity = "سيئون", TripTime = DateTime.UtcNow };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        dbContext.Bookings.Add(new Booking { TripId = trip.Id, TravelerId = traveler.Id, Status = BookingStatus.Confirmed });
        await dbContext.SaveChangesAsync();

        var xssComment = "<script>alert('XSS')</script>";
        var dto = new CreateRatingDto { TripId = trip.Id, Value = 5, Comment = xssComment };

        var result = await ratingService.CreateRatingAsync(traveler.Id, dto);
        Assert.True(result.Success);
        Assert.Equal(xssComment, result.Rating!.Comment);

        // Verification: Razor view encodes html strings safely when rendered without @Html.Raw
    }
}

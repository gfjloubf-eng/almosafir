using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlMosafer.Tests;

public class PaymentNotificationMessagingTest
{
    private DbContextOptions<AlMosaferDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AlMosaferDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetPayment_ValidBookingOwner_ReturnsPaymentDetails()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "سائق 1", Email = "d1@test.com", Role = UserRole.Driver };
        var traveler = new User { Name = "مسافر 1", Email = "t1@test.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "صنعاء", ToCity = "عدن", TripTime = DateTime.Now.AddDays(1), Seats = 4, PricePerSeat = 10000.00m };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = traveler.Id, SeatsBooked = 2, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var paymentService = new PaymentService(dbContext);
        await paymentService.ProcessBookingPaymentAsync(booking.Id, 20000.00m);

        // Act: Traveler accesses their own payment
        var paymentDetails = await paymentService.GetPaymentByBookingIdAsync(traveler.Id, booking.Id);

        Assert.NotNull(paymentDetails);
        Assert.Equal(20000.00m, paymentDetails.Amount);
        Assert.Equal("مسافر 1", paymentDetails.TravelerName);
    }

    [Fact]
    public async Task GetPayment_UnauthorizedOtherUser_ReturnsNullIDORGuard()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "سائق 1", Email = "d1@test.com", Role = UserRole.Driver };
        var travelerA = new User { Name = "مسافر أ", Email = "ta@test.com", Role = UserRole.Traveler };
        var travelerB = new User { Name = "مسافر ب", Email = "tb@test.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, travelerA, travelerB);
        await dbContext.SaveChangesAsync();

        var trip = new Trip { DriverId = driver.Id, FromCity = "تعز", ToCity = "صنعاء", TripTime = DateTime.Now.AddDays(1), Seats = 4, PricePerSeat = 10000.00m };
        dbContext.Trips.Add(trip);
        await dbContext.SaveChangesAsync();

        var booking = new Booking { TripId = trip.Id, TravelerId = travelerA.Id, SeatsBooked = 1, Status = BookingStatus.Confirmed };
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        var paymentService = new PaymentService(dbContext);
        await paymentService.ProcessBookingPaymentAsync(booking.Id, 10000.00m);

        // Act: Traveler B attempts to access Traveler A's payment details
        var unauthorizedResult = await paymentService.GetPaymentByBookingIdAsync(travelerB.Id, booking.Id);

        // Assert: IDOR guard rejects access
        Assert.Null(unauthorizedResult);
    }

    [Fact]
    public async Task SendMessage_ParticipantUser_SendsMessageSuccessfully()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق علي", Email = "ali@driver.com", Role = UserRole.Driver };
        var traveler = new User { Name = "المسافر مراد", Email = "morad@traveler.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, traveler);
        await dbContext.SaveChangesAsync();

        var conversation = new Conversation { BookingId = 1, TripId = 1, DriverId = driver.Id, TravelerId = traveler.Id, CreatedAt = DateTime.UtcNow, LastMessageAt = DateTime.UtcNow };
        dbContext.Conversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var messageService = new MessageService(dbContext);

        var sendDto = new SendMessageDto
        {
            ConversationId = conversation.Id,
            Content = "السلام عليكم، كم كمية الأمتعة المسموح بها؟"
        };

        var sendResult = await messageService.SendMessageAsync(traveler.Id, sendDto);

        Assert.True(sendResult.Success);
        Assert.NotNull(sendResult.MessageId);

        var messages = await messageService.GetConversationMessagesAsync(traveler.Id, conversation.Id);
        Assert.Single(messages);
        Assert.Equal("السلام عليكم، كم كمية الأمتعة المسموح بها؟", messages.First().Content);
    }

    [Fact]
    public async Task SendMessage_NonParticipantUser_FailsIDORGuard()
    {
        var options = CreateInMemoryOptions();
        using var dbContext = new AlMosaferDbContext(options);

        var driver = new User { Name = "السائق علي", Email = "ali@driver.com", Role = UserRole.Driver };
        var travelerA = new User { Name = "مسافر أ", Email = "ta@test.com", Role = UserRole.Traveler };
        var intruder = new User { Name = "مستخدم متطفل", Email = "intruder@test.com", Role = UserRole.Traveler };
        dbContext.Users.AddRange(driver, travelerA, intruder);
        await dbContext.SaveChangesAsync();

        var conversation = new Conversation { BookingId = 1, TripId = 1, DriverId = driver.Id, TravelerId = travelerA.Id, CreatedAt = DateTime.UtcNow, LastMessageAt = DateTime.UtcNow };
        dbContext.Conversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var messageService = new MessageService(dbContext);

        var sendDto = new SendMessageDto
        {
            ConversationId = conversation.Id,
            Content = "محاولة تطفل"
        };

        var sendResult = await messageService.SendMessageAsync(intruder.Id, sendDto);

        Assert.False(sendResult.Success);
        Assert.Contains("لا تملك الصلاحية", sendResult.Message);
    }
}

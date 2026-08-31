using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AlMosaferDbContext _dbContext;

    public PaymentService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Payment> ProcessBookingPaymentAsync(int bookingId, decimal amount)
    {
        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = amount,
            Status = PaymentStatus.Paid,
            TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return payment;
    }

    public async Task<PaymentDetailsDto?> GetPaymentByBookingIdAsync(int userId, int bookingId)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Trip)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Traveler)
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment == null) return null;

        var booking = payment.Booking;
        var user = await _dbContext.Users.FindAsync(userId);

        // Ownership Guard: Traveler, Driver of Trip, or Admin
        if (booking.TravelerId != userId && booking.Trip.DriverId != userId && (user == null || user.Role != UserRole.Admin))
        {
            return null; // IDOR Protection
        }

        var travelerName = booking.Traveler != null ? booking.Traveler.Name : "مسافر";

        return new PaymentDetailsDto
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            TripRoute = $"{booking.Trip.FromCity} ← {booking.Trip.ToCity}",
            TravelerName = travelerName
        };
    }
}

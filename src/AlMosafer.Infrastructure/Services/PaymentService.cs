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
    private readonly IPaymentGateway _gateway;

    public PaymentService(AlMosaferDbContext dbContext, IPaymentGateway? gateway = null)
    {
        _dbContext = dbContext;
        _gateway = gateway ?? new MockPaymentGateway();
    }

    public async Task<Payment> ProcessBookingPaymentAsync(int bookingId, decimal amount)
    {
        // الدفع عبر طبقة التجريد — حالياً محاكاة، ولاحقاً أي بوابة محلية حقيقية
        var charge = await _gateway.ChargeAsync(amount, $"BOOKING-{bookingId}");

        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = amount,
            Status = charge.Success ? PaymentStatus.Paid : PaymentStatus.Failed,
            TransactionId = charge.TransactionId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return payment;
    }

    public async Task<Payment> RegisterCashPaymentAsync(int bookingId, decimal amount)
    {
        var payment = new Payment
        {
            BookingId = bookingId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            TransactionId = null,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return payment;
    }

    public async Task<(bool Success, string Message)> ConfirmCashReceivedAsync(int driverId, int paymentId)
    {
        var payment = await _dbContext.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b.Trip)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
        {
            return (false, "المعاملة غير موجودة.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (payment.Booking.Trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لتأكيد هذه المعاملة.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return (false, "هذه المعاملة ليست بانتظار الاستلام (مدفوعة أو ملغاة سلفاً).");
        }

        payment.Status = PaymentStatus.Paid;
        payment.TransactionId = $"CASH-{payment.BookingId:D6}";
        await _dbContext.SaveChangesAsync();

        return (true, "تم تأكيد استلام المبلغ نقداً. نتمنى رحلة مباركة للجميع!");
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

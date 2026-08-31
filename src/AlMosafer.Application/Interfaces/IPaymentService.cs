using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Domain.Entities;

namespace AlMosafer.Application.Interfaces;

public interface IPaymentService
{
    Task<Payment> ProcessBookingPaymentAsync(int bookingId, decimal amount);
    Task<Payment> RegisterCashPaymentAsync(int bookingId, decimal amount);
    Task<(bool Success, string Message)> ConfirmCashReceivedAsync(int driverId, int paymentId);
    Task<PaymentDetailsDto?> GetPaymentByBookingIdAsync(int userId, int bookingId);
}

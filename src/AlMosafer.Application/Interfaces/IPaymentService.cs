using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Domain.Entities;

namespace AlMosafer.Application.Interfaces;

public interface IPaymentService
{
    Task<Payment> ProcessBookingPaymentAsync(int bookingId, decimal amount);
    Task<PaymentDetailsDto?> GetPaymentByBookingIdAsync(int userId, int bookingId);
}

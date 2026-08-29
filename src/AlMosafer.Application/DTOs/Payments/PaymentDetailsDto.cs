using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Payments;

public class PaymentDetailsDto
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TripRoute { get; set; } = string.Empty;
    public string TravelerName { get; set; } = string.Empty;
}

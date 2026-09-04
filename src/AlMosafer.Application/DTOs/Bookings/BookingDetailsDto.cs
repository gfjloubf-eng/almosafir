using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Bookings;

public class BookingDetailsDto
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime TripTime { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public int TravelerId { get; set; }
    public string TravelerName { get; set; } = string.Empty;
    public string? TravelerPhone { get; set; }
    public int SeatsBooked { get; set; }
    public decimal PricePerSeat { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    /// <summary>حالة الرحلة نفسها — يحدد فصول «القصة الحية» (انطلق/اكتمل).</summary>
    public TripStatus TripStatus { get; set; }
    public DateTime BookingTime { get; set; }
}

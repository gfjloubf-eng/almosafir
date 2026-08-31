using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Bookings;

public class ManifestPassengerDto
{
    public int BookingId { get; set; }
    public string TravelerName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int SeatsBooked { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public bool IsBoarded { get; set; }
}

public class TripManifestDto
{
    public int TripId { get; set; }
    public string Route { get; set; } = string.Empty;
    public DateTime TripTime { get; set; }
    public int TotalSeats { get; set; }
    public int SeatsBookedTotal { get; set; }
    public int BoardedCount { get; set; }
    public int PendingBoardCount { get; set; }
    public decimal CashDueTotal { get; set; }
    public List<ManifestPassengerDto> Passengers { get; set; } = new();
}

using AlMosafer.Domain.Enums;

namespace AlMosafer.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int TravelerId { get; set; }
    public int SeatsBooked { get; set; } = 1;
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTime BookingTime { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Trip Trip { get; set; } = null!;
    public User Traveler { get; set; } = null!;
    public Payment? Payment { get; set; }
    public Conversation? Conversation { get; set; }
}

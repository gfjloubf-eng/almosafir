namespace AlMosafer.Domain.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int? BookingId { get; set; }
    public int TripId { get; set; }
    public int DriverId { get; set; }
    public int TravelerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }

    // Navigation Properties
    public Booking? Booking { get; set; }
    public Trip Trip { get; set; } = null!;
    public User Driver { get; set; } = null!;
    public User Traveler { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

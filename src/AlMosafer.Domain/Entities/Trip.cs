using AlMosafer.Domain.Enums;

namespace AlMosafer.Domain.Entities;

public class Trip
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime TripTime { get; set; }
    public int Seats { get; set; } = 1;
    public decimal PricePerSeat { get; set; } = 0.00m;
    public string? Description { get; set; }
    public string? VehicleInfo { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User Driver { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}

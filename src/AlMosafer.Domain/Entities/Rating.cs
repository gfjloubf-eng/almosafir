namespace AlMosafer.Domain.Entities;

public class Rating
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int TravelerId { get; set; }
    public int DriverId { get; set; }
    public int Value { get; set; } // 1 to 5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Trip Trip { get; set; } = null!;
    public User Traveler { get; set; } = null!;
    public User Driver { get; set; } = null!;
}

namespace AlMosafer.Application.DTOs.Ratings;

public class RatingDetailsDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public int TravelerId { get; set; }
    public string TravelerName { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int Value { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
}

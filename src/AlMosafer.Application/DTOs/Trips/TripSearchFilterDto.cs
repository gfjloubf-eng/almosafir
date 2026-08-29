namespace AlMosafer.Application.DTOs.Trips;

public class TripSearchFilterDto
{
    public string? FromCity { get; set; }
    public string? ToCity { get; set; }
    public DateTime? Date { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool AvailableSeatsOnly { get; set; } = true;
}

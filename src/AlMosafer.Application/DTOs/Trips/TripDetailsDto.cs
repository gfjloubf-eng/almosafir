using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Trips;

public class TripDetailsDto
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public float DriverRating { get; set; }
    public string? PlateNumber { get; set; }
    public string? VehicleModel { get; set; }
    public string FromCity { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public DateTime TripTime { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal PricePerSeat { get; set; }
    public string? Description { get; set; }
    public string? VehicleInfo { get; set; }
    public TripStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

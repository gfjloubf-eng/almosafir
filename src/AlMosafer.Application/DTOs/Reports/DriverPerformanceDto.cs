namespace AlMosafer.Application.DTOs.Reports;

public class DriverPerformanceDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public int TripsCount { get; set; }
    public int BookingsCount { get; set; }
    public int SeatsBookedCount { get; set; }
    public double AverageRating { get; set; }
    public decimal TotalEarnings { get; set; }
}

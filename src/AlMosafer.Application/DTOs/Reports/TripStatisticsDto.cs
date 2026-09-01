namespace AlMosafer.Application.DTOs.Reports;

public class TripStatisticsDto
{
    public int TotalTrips { get; set; }
    public int ActiveTrips { get; set; }
    public int CompletedTrips { get; set; }
    public int CancelledTrips { get; set; }
    public decimal AveragePricePerSeat { get; set; }
    public double AverageSeatsPerTrip { get; set; }
    public int TotalSeats { get; set; }
    public int TotalSeatsBooked { get; set; }
    public string TopOriginCity { get; set; } = "غير محدد";
    public string TopDestinationCity { get; set; } = "غير محدد";
}

namespace AlMosafer.Application.DTOs.Reports;

public class RouteStatisticsDto
{
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public string RouteName => $"{FromCity} ← {ToCity}";
    public int TripsCount { get; set; }
    public int BookingsCount { get; set; }
    public int SeatsBookedCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

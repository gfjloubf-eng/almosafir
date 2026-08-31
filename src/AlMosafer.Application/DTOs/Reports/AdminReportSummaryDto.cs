namespace AlMosafer.Application.DTOs.Reports;

public class AdminReportSummaryDto
{
    public ReportFilterDto ActiveFilter { get; set; } = new();
    public UserStatisticsDto UserStats { get; set; } = new();
    public TripStatisticsDto TripStats { get; set; } = new();
    public BookingStatisticsDto BookingStats { get; set; } = new();
    public PaymentStatisticsDto PaymentStats { get; set; } = new();
    public RatingStatisticsDto RatingStats { get; set; } = new();
    public List<RouteStatisticsDto> PopularRoutes { get; set; } = new();
    public List<DriverPerformanceDto> TopDrivers { get; set; } = new();
    public List<TimeSeriesPointDto> BookingTrend { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

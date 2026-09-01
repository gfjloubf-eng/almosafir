namespace AlMosafer.Application.DTOs.Reports;

public class UserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TravelersCount { get; set; }
    public int DriversCount { get; set; }
    public int AdminsCount { get; set; }
    public double TravelersPercentage => TotalUsers > 0 ? Math.Round((double)TravelersCount / TotalUsers * 100, 1) : 0;
    public double DriversPercentage => TotalUsers > 0 ? Math.Round((double)DriversCount / TotalUsers * 100, 1) : 0;
    public double AdminsPercentage => TotalUsers > 0 ? Math.Round((double)AdminsCount / TotalUsers * 100, 1) : 0;
}

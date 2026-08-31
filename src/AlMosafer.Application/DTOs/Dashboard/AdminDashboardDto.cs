namespace AlMosafer.Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TravelersCount { get; set; }
    public int DriversCount { get; set; }
    public int TotalTrips { get; set; }
    public int ActiveTrips { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalPaymentsAmount { get; set; }
    public int UnreadNotificationsCount { get; set; }
}

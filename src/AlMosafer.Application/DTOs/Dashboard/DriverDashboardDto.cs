using AlMosafer.Application.DTOs.Trips;

namespace AlMosafer.Application.DTOs.Dashboard;

public class DriverDashboardDto
{
    public int TotalTrips { get; set; }
    public int ActiveTrips { get; set; }
    public int TotalSeatsBooked { get; set; }
    public decimal TotalEarnings { get; set; }
    public int UnreadNotificationsCount { get; set; }
    public int ConversationsCount { get; set; }
    public IEnumerable<TripDetailsDto> RecentTrips { get; set; } = new List<TripDetailsDto>();
}

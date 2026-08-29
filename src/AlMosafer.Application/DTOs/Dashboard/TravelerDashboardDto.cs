using AlMosafer.Application.DTOs.Bookings;

namespace AlMosafer.Application.DTOs.Dashboard;

public class TravelerDashboardDto
{
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int UnreadNotificationsCount { get; set; }
    public int ConversationsCount { get; set; }
    public IEnumerable<BookingDetailsDto> RecentBookings { get; set; } = new List<BookingDetailsDto>();
}

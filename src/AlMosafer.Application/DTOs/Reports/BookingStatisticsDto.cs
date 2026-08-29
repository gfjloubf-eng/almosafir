namespace AlMosafer.Application.DTOs.Reports;

public class BookingStatisticsDto
{
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public double ConfirmationRate => TotalBookings > 0 ? Math.Round((double)ConfirmedBookings / TotalBookings * 100, 1) : 0;
    public double CancellationRate => TotalBookings > 0 ? Math.Round((double)CancelledBookings / TotalBookings * 100, 1) : 0;
    public double AverageSeatsPerBooking { get; set; }
    public int TotalSeatsBooked { get; set; }
}

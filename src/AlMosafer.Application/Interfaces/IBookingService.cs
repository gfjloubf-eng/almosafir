using AlMosafer.Application.DTOs.Bookings;

namespace AlMosafer.Application.Interfaces;

public interface IBookingService
{
    Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(int travelerId, CreateBookingDto dto);
    Task<(bool Success, string Message)> CancelBookingAsync(int userId, int bookingId);
    Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId);
    Task<IEnumerable<BookingDetailsDto>> GetUserBookingsAsync(int travelerId);
    Task<IEnumerable<BookingDetailsDto>> GetTripBookingsAsync(int driverId, int tripId);
}

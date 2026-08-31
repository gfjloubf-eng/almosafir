using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.DTOs.Dashboard;
using AlMosafer.Application.DTOs.Messaging;
using AlMosafer.Application.DTOs.Notifications;
using AlMosafer.Application.DTOs.Payments;
using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserProfileDto>> GetUsersAsync(string? search = null, UserRole? roleFilter = null);
    Task<UserProfileDto?> GetUserDetailsAsync(int userId);
    Task<IEnumerable<TripDetailsDto>> GetTripsAsync(string? origin = null, string? destination = null, int? driverId = null, TripStatus? statusFilter = null);
    Task<IEnumerable<BookingDetailsDto>> GetBookingsAsync(BookingStatus? statusFilter = null);
    Task<IEnumerable<PaymentDetailsDto>> GetPaymentsAsync();
    Task<IEnumerable<RatingDetailsDto>> GetRatingsAsync();
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync();
    Task<IEnumerable<ConversationDto>> GetConversationsAsync();
    Task<AdminSystemHealthDto> GetSystemHealthAsync();
}

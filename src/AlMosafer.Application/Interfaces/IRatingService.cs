using AlMosafer.Application.DTOs.Ratings;

namespace AlMosafer.Application.Interfaces;

public interface IRatingService
{
    Task<(bool Success, string Message, RatingDetailsDto? Rating)> CreateRatingAsync(int travelerId, CreateRatingDto dto);
    Task<DriverRatingSummaryDto> GetDriverRatingSummaryAsync(int driverId);
    Task<IEnumerable<RatingDetailsDto>> GetTripRatingsAsync(int tripId);
    Task<bool> HasTravelerRatedTripAsync(int travelerId, int tripId);
    Task<bool> CanTravelerRateBookingAsync(int travelerId, int bookingId);
}

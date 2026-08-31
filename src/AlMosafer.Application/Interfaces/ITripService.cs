using AlMosafer.Application.DTOs.Trips;

namespace AlMosafer.Application.Interfaces;

public interface ITripService
{
    Task<(bool Success, string Message, int? TripId)> CreateTripAsync(int driverId, CreateTripDto dto);
    Task<(bool Success, string Message)> UpdateTripAsync(int driverId, UpdateTripDto dto);
    Task<(bool Success, string Message)> CancelTripAsync(int driverId, int tripId);
    Task<TripDetailsDto?> GetTripByIdAsync(int tripId);
    Task<IEnumerable<TripDetailsDto>> SearchTripsAsync(TripSearchFilterDto filter);
    Task<IEnumerable<TripDetailsDto>> GetDriverTripsAsync(int driverId);
}

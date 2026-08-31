using AlMosafer.Application.DTOs.Watchlist;

namespace AlMosafer.Application.Interfaces;

public interface IWatchlistService
{
    Task<(bool Success, string Message)> WatchRouteAsync(int userId, string fromCity, string toCity);
    Task<(bool Success, string Message)> UnwatchRouteAsync(int userId, string fromCity, string toCity);
    Task<IReadOnlyList<RouteWatchDto>> GetWatchedRoutesAsync(int userId);
    Task<int> NotifyWatchersForTripAsync(int tripId);
}

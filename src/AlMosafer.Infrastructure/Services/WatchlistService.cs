using System.Text.Json;
using System.Text.Json.Serialization;
using AlMosafer.Application.DTOs.Watchlist;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly AlMosaferDbContext _dbContext;
    private readonly INotificationService _notificationService;

    // المفضلات تُخزَّن في عمود PreferencesJson الموجود أصلاً (بلا أي هجرة مخطط)
    private sealed class Preferences
    {
        [JsonPropertyName("WatchedRoutes")]
        public List<RouteWatchDto> WatchedRoutes { get; set; } = new();
    }

    public WatchlistService(AlMosaferDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    private static Preferences ReadPreferences(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Preferences();
        }

        try
        {
            return JsonSerializer.Deserialize<Preferences>(json) ?? new Preferences();
        }
        catch (JsonException)
        {
            return new Preferences();
        }
    }

    public async Task<(bool Success, string Message)> WatchRouteAsync(int userId, string fromCity, string toCity)
    {
        var from = (fromCity ?? string.Empty).Trim();
        var to = (toCity ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            return (false, "مدينتا الخط مطلوبتان للمراقبة.");
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود.");
        }

        var prefs = ReadPreferences(user.PreferencesJson);
        var exists = prefs.WatchedRoutes.Any(w =>
            string.Equals(w.FromCity, from, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(w.ToCity, to, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            return (false, "هذا الخط موجود بالفعل في قائمة مراقبتك.");
        }

        prefs.WatchedRoutes.Add(new RouteWatchDto { FromCity = from, ToCity = to });
        user.PreferencesJson = JsonSerializer.Serialize(prefs);
        await _dbContext.SaveChangesAsync();

        return (true, $"أصبحت تتابع خط {from} ← {to}. سننبهك فور نزول رحلة جديدة عليه 🌟");
    }

    public async Task<(bool Success, string Message)> UnwatchRouteAsync(int userId, string fromCity, string toCity)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "المستخدم غير موجود.");
        }

        var prefs = ReadPreferences(user.PreferencesJson);
        var removed = prefs.WatchedRoutes.RemoveAll(w =>
            string.Equals(w.FromCity, (fromCity ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(w.ToCity, (toCity ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            return (false, "هذا الخط غير موجود في قائمة مراقبتك.");
        }

        user.PreferencesJson = JsonSerializer.Serialize(prefs);
        await _dbContext.SaveChangesAsync();

        return (true, "أُزيل الخط من قائمة مراقبتك.");
    }

    public async Task<IReadOnlyList<RouteWatchDto>> GetWatchedRoutesAsync(int userId)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return Array.Empty<RouteWatchDto>();
        }

        return ReadPreferences(user.PreferencesJson).WatchedRoutes;
    }

    public async Task<int> NotifyWatchersForTripAsync(int tripId)
    {
        var trip = await _dbContext.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip == null)
        {
            return 0;
        }

        var candidates = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.PreferencesJson != null && u.Id != trip.DriverId)
            .ToListAsync();

        var notified = 0;
        foreach (var user in candidates)
        {
            var match = ReadPreferences(user.PreferencesJson).WatchedRoutes.Any(w =>
                string.Equals(w.FromCity, trip.FromCity, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(w.ToCity, trip.ToCity, StringComparison.OrdinalIgnoreCase));
            if (!match)
            {
                continue;
            }

            await _notificationService.SendNotificationAsync(
                user.Id,
                "رحلة جديدة على خطك المفضل 🌟",
                $"نُشرت رحلة من {trip.FromCity} إلى {trip.ToCity} يوم {trip.TripTime:yyyy-MM-dd} الساعة {trip.TripTime:HH:mm}. أسرع بالحجز قبل اكتمال المقاعد!",
                NotificationType.TripUpdate);
            notified++;
        }

        return notified;
    }
}

using AlMosafer.Application.DTOs.Dashboard;

namespace AlMosafer.Application.Interfaces;

public interface IDashboardService
{
    Task<TravelerDashboardDto> GetTravelerDashboardAsync(int travelerId);
    Task<DriverDashboardDto> GetDriverDashboardAsync(int driverId);
    Task<AdminDashboardDto> GetAdminDashboardAsync(int adminUserId);
}

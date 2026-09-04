using System.Security.Claims;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize(Roles = "Traveler,Admin")]
public class TravelerController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IBookingService _bookingService;

    public TravelerController(IDashboardService dashboardService, IBookingService bookingService)
    {
        _dashboardService = dashboardService;
        _bookingService = bookingService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة المسافر — Traveler Dashboard";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var travelerId))
        {
            return Unauthorized();
        }

        var dashboardData = await _dashboardService.GetTravelerDashboardAsync(travelerId);
        return View(dashboardData);
    }

    /// <summary>
    /// «رحلتي الحية» (P51): القصة السردية للرحلة — خط زمني من 5 فصول
    /// يضيء فصلاً تلو الآخر لحظياً عبر WebSocket مع تقدّم الرحلة الفعلي.
    /// </summary>
    public async Task<IActionResult> Journey()
    {
        ViewData["Title"] = "رحلتي الحية — القصة السردية للرحلة";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var travelerId))
        {
            return Unauthorized();
        }

        var bookings = await _bookingService.GetUserBookingsAsync(travelerId);
        return View(bookings);
    }
}

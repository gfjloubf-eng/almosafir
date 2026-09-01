using System.Security.Claims;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize(Roles = "Driver,Admin")]
public class DriverController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DriverController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة السائق — Driver Dashboard";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var driverId))
        {
            return Unauthorized();
        }

        var dashboardData = await _dashboardService.GetDriverDashboardAsync(driverId);
        return View(dashboardData);
    }
}

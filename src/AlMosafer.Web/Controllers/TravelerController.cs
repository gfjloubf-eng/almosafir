using System.Security.Claims;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize(Roles = "Traveler,Admin")]
public class TravelerController : Controller
{
    private readonly IDashboardService _dashboardService;

    public TravelerController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
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
}

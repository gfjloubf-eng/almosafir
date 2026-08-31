using System.Security.Claims;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize]
public class WatchlistController : Controller
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet]
    public async Task<IActionResult> MyRoutes()
    {
        var routes = await _watchlistService.GetWatchedRoutesAsync(GetCurrentUserId());
        return View(routes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Watch(string fromCity, string toCity, string? returnUrl)
    {
        var result = await _watchlistService.WatchRouteAsync(GetCurrentUserId(), fromCity, toCity);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(MyRoutes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unwatch(string fromCity, string toCity)
    {
        var result = await _watchlistService.UnwatchRouteAsync(GetCurrentUserId(), fromCity, toCity);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(MyRoutes));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

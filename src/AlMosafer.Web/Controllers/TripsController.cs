using System.Security.Claims;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

public class TripsController : Controller
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(TripSearchFilterDto filter)
    {
        var trips = await _tripService.SearchTripsAsync(filter);
        ViewData["Filter"] = filter;
        return View(trips);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);
        if (trip == null)
        {
            return NotFound();
        }
        return View(trip);
    }

    [HttpGet]
    [Authorize(Roles = "Driver,Admin")]
    public IActionResult Create()
    {
        return View(new CreateTripDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Create(CreateTripDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var driverId = GetCurrentUserId();
        var result = await _tripService.CreateTripAsync(driverId, dto);

        if (!result.Success || !result.TripId.HasValue)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.TripId.Value });
    }

    [HttpGet]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> MyTrips()
    {
        var driverId = GetCurrentUserId();
        var trips = await _tripService.GetDriverTripsAsync(driverId);
        return View(trips);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

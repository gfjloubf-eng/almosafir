using System.Security.Claims;
using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

public class RatingsController : Controller
{
    private readonly IRatingService _ratingService;
    private readonly ITripService _tripService;

    public RatingsController(IRatingService ratingService, ITripService tripService)
    {
        _ratingService = ratingService;
        _tripService = tripService;
    }

    [Authorize(Roles = "Traveler")]
    [HttpGet]
    public async Task<IActionResult> Create(int tripId)
    {
        var travelerId = GetCurrentUserId();
        var trip = await _tripService.GetTripByIdAsync(tripId);
        if (trip == null)
        {
            return NotFound();
        }

        var alreadyRated = await _ratingService.HasTravelerRatedTripAsync(travelerId, tripId);
        if (alreadyRated)
        {
            TempData["InfoMessage"] = "لقد قمت بتقييم هذه الرحلة مسبقاً.";
            return RedirectToAction("MyBookings", "Traveler");
        }

        var dto = new CreateRatingDto
        {
            TripId = tripId,
            Value = 5 // Default 5 stars
        };

        ViewData["TripDetails"] = trip;
        return View(dto);
    }

    [Authorize(Roles = "Traveler")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRatingDto dto)
    {
        var travelerId = GetCurrentUserId();
        if (!ModelState.IsValid)
        {
            var trip = await _tripService.GetTripByIdAsync(dto.TripId);
            ViewData["TripDetails"] = trip;
            return View(dto);
        }

        var result = await _ratingService.CreateRatingAsync(travelerId, dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var trip = await _tripService.GetTripByIdAsync(dto.TripId);
            ViewData["TripDetails"] = trip;
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("MyBookings", "Traveler");
    }

    [HttpGet]
    public async Task<IActionResult> DriverSummary(int driverId)
    {
        var summary = await _ratingService.GetDriverRatingSummaryAsync(driverId);
        return View(summary);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

using System.Security.Claims;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using AlMosafer.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlMosafer.Web.Controllers;

public class TripsController : Controller
{
    private readonly ITripService _tripService;
    private readonly IWatchlistService _watchlistService;
    private readonly ILineService _lineService;
    private readonly IHubContext<LiveHub> _liveHub;

    public TripsController(ITripService tripService, IWatchlistService watchlistService, ILineService lineService, IHubContext<LiveHub> liveHub)
    {
        _tripService = tripService;
        _watchlistService = watchlistService;
        _lineService = lineService;
        _liveHub = liveHub;
    }

    [HttpGet]
    public async Task<IActionResult> Index(TripSearchFilterDto filter)
    {
        var trips = await _tripService.SearchTripsAsync(filter);
        ViewData["Filter"] = filter;
        // P43 م٣: اقتراحات المدن الحية من شبكة الخطوط المعتمدة (نفس نمط الرئيسية)
        ViewBag.Cities = await _lineService.GetActiveCitiesAsync();
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
    public async Task<IActionResult> InternalLines()
    {
        var lines = await _tripService.GetInternalLinesAsync();
        return View(lines);
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

        await _watchlistService.NotifyWatchersForTripAsync(result.TripId.Value);

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

    [HttpGet]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var trip = await _tripService.GetTripByIdAsync(id);
        if (trip == null)
        {
            return NotFound();
        }

        var dto = new UpdateTripDto
        {
            TripId = trip.Id,
            FromCity = trip.FromCity,
            FromLocation = trip.FromLocation,
            ToCity = trip.ToCity,
            TripTime = trip.TripTime,
            Seats = trip.TotalSeats,
            PricePerSeat = trip.PricePerSeat,
            Description = trip.Description,
            VehicleInfo = trip.VehicleInfo,
            Status = trip.Status
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Edit(UpdateTripDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _tripService.UpdateTripAsync(GetCurrentUserId(), dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(dto);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(MyTrips));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _tripService.CancelTripAsync(GetCurrentUserId(), id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(MyTrips));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Start(int id)
    {
        var result = await _tripService.StartTripAsync(GetCurrentUserId(), id);
        if (result.Success)
        {
            // P50/P51: بث لحظة الانطلاق — للوحة الانطلاق ولمن يتابع «رحلته الحية» معاً
            var payload = new { tripId = id };
            await _liveHub.Clients.Group("board").SendAsync("TripStarted", payload);
            await _liveHub.Clients.Group($"trip-{id}").SendAsync("TripStarted", payload);
        }
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(MyTrips));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Complete(int id)
    {
        var result = await _tripService.CompleteTripAsync(GetCurrentUserId(), id);
        if (result.Success)
        {
            // P51: بث لحظة الوصول — يُضيء الفصل الأخير في «رحلتي الحية» ويُظهر دعوة التقييم
            var payload = new { tripId = id };
            await _liveHub.Clients.Group("board").SendAsync("TripCompleted", payload);
            await _liveHub.Clients.Group($"trip-{id}").SendAsync("TripCompleted", payload);
        }
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(MyTrips));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

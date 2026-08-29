using System.Security.Claims;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "بيانات الحجز غير صحيحة.";
            return RedirectToAction("Details", "Trips", new { id = dto.TripId });
        }

        var travelerId = GetCurrentUserId();
        var result = await _bookingService.CreateBookingAsync(travelerId, dto);

        if (!result.Success || !result.BookingId.HasValue)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Details", "Trips", new { id = dto.TripId });
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Receipt), new { id = result.BookingId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var travelerId = GetCurrentUserId();
        var bookings = await _bookingService.GetUserBookingsAsync(travelerId);
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (booking.TravelerId != userId && User.IsInRole("Admin") == false)
        {
            return Forbid();
        }

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int bookingId)
    {
        var userId = GetCurrentUserId();
        var result = await _bookingService.CancelBookingAsync(userId, bookingId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
        }
        else
        {
            TempData["SuccessMessage"] = result.Message;
        }

        return RedirectToAction(nameof(MyBookings));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

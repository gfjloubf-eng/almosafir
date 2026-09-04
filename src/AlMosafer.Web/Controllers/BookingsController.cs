using System.Security.Claims;
using QRCoder;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.Interfaces;
using AlMosafer.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlMosafer.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly ITicketSignatureService _ticketSignature;
    private readonly ITripService _tripService;
    private readonly IHubContext<LiveHub> _liveHub;

    public BookingsController(IBookingService bookingService, ITicketSignatureService ticketSignature, ITripService tripService, IHubContext<LiveHub> liveHub)
    {
        _ticketSignature = ticketSignature;
        _bookingService = bookingService;
        _tripService = tripService;
        _liveHub = liveHub;
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

        // P50 «عصر التطوير»: بث المقاعد المتبقية لحظياً لكل من يتابع الرحلة (شاشات أخرى تتحدث بلا تحديث يدوي)
        var availableSeats = await _tripService.GetAvailableSeatsAsync(dto.TripId);
        await _liveHub.Clients.Group($"trip-{dto.TripId}")
            .SendAsync("SeatsUpdated", new { tripId = dto.TripId, availableSeats });

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

        // P43 التذكرة الموقّعة: QR يحمل رابط تحقق موقّعاً (HMAC بالرمز السري) — بلا الرمز من القاعدة لا تُزوَّر.
        // الحجوزات الأقدم من الميزة تسقط إلى رابط الإيصال القديم وتُعرَش بصدق «غير موقّعة» عند المسح.
        var signature = await _ticketSignature.CreateSignatureAsync(id);
        var scanTarget = signature is not null
            ? Url.Action("Verify", "Tickets", new { id, sig = signature }, Request.Scheme) ?? string.Empty
            : Url.Action(nameof(Receipt), "Bookings", new { id }, Request.Scheme) ?? string.Empty;
        ViewBag.TicketSigned = signature is not null;
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(scanTarget, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        ViewBag.QrCodeDataUri = "data:image/png;base64," + Convert.ToBase64String(qrCode.GetGraphic(6));

        return View(booking);
    }

    [HttpGet]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Manifest(int tripId)
    {
        var manifest = await _bookingService.GetTripManifestAsync(GetCurrentUserId(), tripId);
        if (manifest == null)
        {
            return Forbid(); // كشف الركوب لسائق الرحلة أو الإدمن فقط
        }
        return View(manifest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<IActionResult> Board(int bookingId, int tripId)
    {
        var result = await _bookingService.MarkBoardedAsync(GetCurrentUserId(), bookingId);
        if (result.Success)
        {
            // P51: بث لحظة الصعود — يُضيء فصل «تم الصعود» في «رحلتي الحية» لحظياً
            await _liveHub.Clients.Group($"trip-{tripId}").SendAsync("BookingBoarded", new { tripId, bookingId });
        }
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Manifest), new { tripId });
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

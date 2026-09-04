using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

/// <summary>
/// لوحة الانطلاق الحية (P50 «عصر التطوير»): صفحة عامة بأسلوب لوحات المطارات —
/// الرحلات القادمة بعدّادات زمنية تتناقص كل ثانية، وحالة الصف تنقلب لحظياً عند انطلاق الرحلة.
/// </summary>
public class BoardController : Controller
{
    private readonly ITripService _tripService;

    public BoardController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // الرحلات المفتوحة القادمة فقط، مرتبة زمنياً — مع إظهار حتى الكاملة (لوحة إرشاد لا بحث)
        var trips = await _tripService.SearchTripsAsync(new TripSearchFilterDto { AvailableSeatsOnly = false });
        return View(trips);
    }
}

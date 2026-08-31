using System.Security.Claims;
using AlMosafer.Application.DTOs.Reports;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IAdminService _adminService;
    private readonly IReportingService _reportingService;

    public AdminController(IDashboardService dashboardService, IAdminService adminService, IReportingService reportingService)
    {
        _dashboardService = dashboardService;
        _adminService = adminService;
        _reportingService = reportingService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة الإدارة — Admin Dashboard";

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var adminUserId))
        {
            return Unauthorized();
        }

        var dashboardData = await _dashboardService.GetAdminDashboardAsync(adminUserId);
        return View(dashboardData);
    }

    [HttpGet]
    public async Task<IActionResult> Users(string? search = null, UserRole? role = null)
    {
        ViewData["Title"] = "إدارة مستخدمي المنظومة — نظام المسافر";
        ViewData["Search"] = search;
        ViewData["RoleFilter"] = role;

        var users = await _adminService.GetUsersAsync(search, role);
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(int id)
    {
        ViewData["Title"] = "تفاصيل حساب المستخدم — نظام المسافر";

        var user = await _adminService.GetUserDetailsAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Trips(string? origin = null, string? destination = null, int? driverId = null, TripStatus? status = null)
    {
        ViewData["Title"] = "مراقبة ورحلات المنظومة — نظام المسافر";
        ViewData["Origin"] = origin;
        ViewData["Destination"] = destination;
        ViewData["StatusFilter"] = status;

        var trips = await _adminService.GetTripsAsync(origin, destination, driverId, status);
        return View(trips);
    }

    [HttpGet]
    public async Task<IActionResult> Bookings(BookingStatus? status = null)
    {
        ViewData["Title"] = "مراقبة حجوزات المنظومة — نظام المسافر";
        ViewData["StatusFilter"] = status;

        var bookings = await _adminService.GetBookingsAsync(status);
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Payments()
    {
        ViewData["Title"] = "مراقبة المعاملات المالية — نظام المسافر";

        var payments = await _adminService.GetPaymentsAsync();
        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> Ratings()
    {
        ViewData["Title"] = "مراقبة تقييمات السائقين والركاب — نظام المسافر";

        var ratings = await _adminService.GetRatingsAsync();
        return View(ratings);
    }

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        ViewData["Title"] = "مراقبة تنبيهات وإشعارات النظام — نظام المسافر";

        var notifications = await _adminService.GetNotificationsAsync();
        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> Conversations()
    {
        ViewData["Title"] = "مراقبة محادثات السائقين والركاب — نظام المسافر";

        var conversations = await _adminService.GetConversationsAsync();
        return View(conversations);
    }

    [HttpGet]
    public async Task<IActionResult> SystemHealth()
    {
        ViewData["Title"] = "حالة وصحة المنظومة البرمجية — System Health";

        var health = await _adminService.GetSystemHealthAsync();
        return View(health);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? fromDate = null, DateTime? toDate = null, UserRole? role = null)
    {
        ViewData["Title"] = "التقارير والإحصائيات البيانية — نظام المسافر";

        var filter = new ReportFilterDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            RoleFilter = role
        };

        var reportSummary = await _reportingService.GetAdminReportSummaryAsync(filter);
        return View(reportSummary);
    }
}

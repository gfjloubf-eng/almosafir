using System.Security.Claims;
using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return View(notifications);
    }

    // P40: عدّاد خفيف لشارة الجرس عند فتح أي صفحة (يشقّه realtime.js)
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var userId = GetCurrentUserId();
        var unread = await _notificationService.GetUnreadCountAsync(userId);
        return Json(new { unread });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(userId, notificationId);
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

/// <summary>
/// تحقق التذاكر عند المسح (P43): يفتحه السائق من كاميرا هاتفه أثناء الصعود.
/// مخوّل للسائقين والإدارة فقط — رابط المسح نفسه عام لكنه يطلب تسجيل الدخول بدور مناسب.
/// </summary>
[Authorize(Roles = "Driver,Admin")]
public class TicketsController : Controller
{
    private readonly ITicketSignatureService _ticketSignature;

    public TicketsController(ITicketSignatureService ticketSignature)
    {
        _ticketSignature = ticketSignature;
    }

    [HttpGet]
    public async Task<IActionResult> Verify(int id, string? sig)
    {
        var result = await _ticketSignature.VerifyAsync(id, sig);
        return View(result);
    }
}
